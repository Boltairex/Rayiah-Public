using Discord;
using System;

namespace Rayiah.Managers
{
    public static class Status
    {
        public static IActivity CurrentStatus { get; private set; }
        static NormalStatus defaultStatus;

        static Status() {
            defaultStatus = new NormalStatus();
        }

        public static void SetDefaultStatus() {
            CurrentStatus = defaultStatus;
            RayiahCore.GetInstance().client.SetActivityAsync(CurrentStatus).ContinueWith((obj) => {
                Console.WriteLine("Zmieniono");
            });
        }

        public static void SetCustomStatus(IActivity customStatus) {
            CurrentStatus = customStatus;
            RayiahCore.GetInstance().client.SetActivityAsync(CurrentStatus);
        }
    }

    public class NormalStatus : IActivity
    {
        public string Name => "Chilluje bombę";

        public string Details => "Piwo to alkohol.";
        
        public ActivityType Type => ActivityType.CustomStatus;

        public ActivityProperties Flags => ActivityProperties.Instance;
    }
}
