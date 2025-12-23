using System.Threading.Tasks;
using Default.EXtensions;
using DreamPoeBot.Loki.Bot;

namespace Default.MapBot
{
    public class TravelToLabTask : ITask
    {
        public async Task<bool> Run()
        {
            // Templar Laboratory is no longer used in modern PoE
            // Always use hideout instead
            return false;
        }

        #region Unused interface methods

        public MessageResult Message(Message message)
        {
            return MessageResult.Unprocessed;
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Tick()
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public string Name => "TravelToLabTask";
        public string Description => "Task for traveling to The Eternal Laboratory.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}