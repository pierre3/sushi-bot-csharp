using Line.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SushiBotCSharp
{
    public class TaklContext
    {
        private TableStorage<TalkStatus> talkStatus;

        private IList<TalkSkill> talkSkills = new List<TalkSkill>();
        
        public TaklContext()
        {
        }
        
        private async Task InitializeAsync(string tableStorageConnectionString)
        {
            talkStatus = await TableStorage<TalkStatus>.CreateAsync(tableStorageConnectionString, "talkStatus");
        }

        public static async Task<TaklContext> CreateAsync(string tableStorageConnectionString)
        {
            var instance = new TaklContext();
            await instance.InitializeAsync(tableStorageConnectionString);
            return instance;
        }

        public void RegisterTalkSkill(TalkSkill skill)
        {
            if(skill == null) { throw new ArgumentNullException(nameof(skill)); }
            talkSkills.Add(skill);
        }

        public async Task<bool> ProcessAsync(LineMessagingClient line, NlpResult nlpResult, string userId, string replyToken)
        {
            foreach (var skill in talkSkills)
            {
                try
                {
                    if( await ProcessAsync_Impl(skill, line, nlpResult, userId, replyToken))
                    {
                        return true;
                    }
                }
                catch
                {
                    await line.ReplyMessageAsync(replyToken, "申し訳ありません。システムに問題が発生しました。");
                    await talkStatus.DeleteAsync(userId, skill.Name);
                    throw;
                }
            }
            return false;
        }

        protected async Task<bool> ProcessAsync_Impl(TalkSkill skill, LineMessagingClient line, NlpResult nlpResult, string userId, string replyToken)
        {            
            
            var status = await talkStatus.FindAsync(userId, skill.Name);
            if (status == null)
            {
                if (nlpResult.Intent != skill.Name) { return false; }

                status = new TalkStatus(userId, skill.Name, nlpResult.Entities);
                skill.Start(status);
            }
            else
            {
                foreach (var entity in nlpResult.Entities)
                {
                    status.Parameters[entity.Key] = entity.Value;
                }
            }
            
            //問い合わせ中のパラメータを処理する
            if (skill.RequierdParameters.TryGetValue(status.WaitingParameter, out TalkParameter param))
            {
                //ユーザからの返答に対する処理を行う。
                if (TalkReactionResult.Reject == param.Reaction(skill.ReplyMessages, status))
                {
                    //要求したパラメータが取得できなかったらReject時のメッセージを表示して抜ける
                    await line.ReplyMessageAsync(replyToken, skill.ReplyMessages);
                    return true;
                }
            }

            //問い合わせ中のパラメータなし
            status.WaitingParameter = "";

            //必須パラメータがそろっているか確認
            foreach (var reqParam in skill.RequierdParameters)
            {
                //足りない必須パラメータがあったら問い合わせする(パラメータの登録順)
                if (!status.Parameters.TryGetValue(reqParam.Key, out string value))
                {
                    status.WaitingParameter = reqParam.Key;
                    reqParam.Value.Confirm(skill.ReplyMessages);
                    await line.ReplyMessageAsync(replyToken, skill.ReplyMessages);
                    await talkStatus.UpdateAsync(status);
                    return true;
                }
            }

            //全ての必須パラメータが揃った
            skill.Finish(status);
            await line.ReplyMessageAsync(replyToken, skill.ReplyMessages);
            await talkStatus.DeleteAsync(status.PartitionKey, status.RowKey);
            return true;
        }

    }
}