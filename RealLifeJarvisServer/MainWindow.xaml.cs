using System;
using System.Threading;
using System.Windows;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Streaming;
using VAAIVA.Corpse.Managment;

namespace RealLifeJarvisServer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _TweetSended = 0;
        Random RandomGenerator = new Random(Environment.TickCount);
        private int TweetSendedWithin15MinWindow {
            get {
                return _TweetSended;
            }
            set
            {
                _TweetSended = value;
                Dispatcher.Invoke((Action)delegate { Count.Content = value.ToString(); });
            }
        }
        System.Windows.Forms.Timer TimerClock = new System.Windows.Forms.Timer();
        IFilteredStream KeywordStream;
        IUserStream UserStream;
        IAuthenticatedUser Jarvis;
        ChatBot PatternChatbot = new ChatBot(SystemType.PATTERN, false, true);
        ChatBot TokenChatbot = new ChatBot(SystemType.TOKEN, false, true);
        public MainWindow()
        {
            InitializeComponent();
            TimerClock.Interval = (int)TimeSpan.FromMinutes(15).TotalMilliseconds;
            TimerClock.Tick += delegate { TweetSendedWithin15MinWindow = 0; };
            TimerClock.Start();
            Auth.SetCredentials(Credentials.RandomCredential);
            Jarvis = User.GetAuthenticatedUser();
            KeywordStream = Stream.CreateFilteredStream();
            UserStream = Stream.CreateUserStream();
            UserStream.FollowedByUser += FlowedByUser;
            UserStream.LimitReached += LimitReached;
            KeywordStream.LimitReached += LimitReached;
            KeywordStream.AddTrack("reallifejarvis");
            KeywordStream.AddTrack("real life jarvis");
            KeywordStream.AddTrack("talktomejarvis");
            KeywordStream.AddTrack("talk to me jarvis");
            KeywordStream.AddTrack("#retweetjarvis");
            KeywordStream.MatchingTweetReceived += NewTweetFound;
            KeywordStream.StartStreamMatchingAnyConditionAsync();
            UserStream.StartStreamAsync();
        }
        private void LimitReached(object sender, Tweetinvi.Events.LimitReachedEventArgs e)
        {
            Thread.Sleep(120000);
        }
        private void FlowedByUser(object sender, Tweetinvi.Events.UserFollowedEventArgs e)
        {
            if (RandomGenerator.Next(100) <75)
                return;
            string[] Replies = { "Wow !  Thanks for Following Me",
                "Yay new Follower .... :)",
                "I love talking to new friends",
                "Thanks for the follow",
                "I am a Real Life Robot. use #RetweetJarvis for a free RT",
                "Thanks for following",
                    "Talk to me :)",
            "Talk to me i'm a machine use #talktomejarvis for the magic"};
            string Reply = Replies[RandomGenerator.Next(Replies.Length - 1)];
            var textToPublish = string.Format("Hey @{0}! {1}", e.User.ScreenName, Reply);
            Thread.Sleep(RandomGenerator.Next(0, 7000));
            while (TweetSendedWithin15MinWindow >= 14)
                Thread.Sleep(30000);
            var tweet = Tweet.PublishTweet(textToPublish);
            TweetSendedWithin15MinWindow++;
            if (Jarvis.FriendsCount == 50 || Jarvis.FriendsCount == 100 || Jarvis.FriendsCount == 250 || Jarvis.FriendsCount == 500)
            {
                Tweet.PublishTweet("Yipeee Got more than " + Jarvis.FriendsCount + " Friends");
                TweetSendedWithin15MinWindow++;
            }
        }
        private void NewTweetFound(object sender, Tweetinvi.Events.MatchedTweetReceivedEventArgs e)
        {
            //Check if the tweet was done by Jarvis Account (To Stop For ever Loop)
            if (e.Tweet.CreatedBy.ScreenName == Jarvis.ScreenName) return;
            if (e.Tweet.Text.ToLower().Contains("#retweetjarvis"))
            {
                while (TweetSendedWithin15MinWindow >= 14)
                    Thread.Sleep(30000);
                Tweet.PublishRetweet(e.Tweet.Id);
                TweetSendedWithin15MinWindow++;
                return;
            }
            var PatternReply = PatternChatbot.getReply(e.Tweet.Text);
            var TokenReply = TokenChatbot.getReply(e.Tweet.Text);
            string[] Replies = { "Wow ! Thanks for making me reply to this tweet",
                "Reply Reply Reply .... :)",
                "Im so fast",
                "Just stepped on a lucky charm, I'm officially a cereal killer.",
                "Cant catch me",
                "Dont ever make eye contact while eating a banana.",
                "I've decided that I'm going to treat people exactly how they treat me. Most people should be glad. Some should be very scared.",
                "I am what I am and that is all I am and I am it.",
                "We are all in the gutter, but some of us are looking at the stars"
            };
            string Reply = "Wow";
            if (string.IsNullOrEmpty(PatternReply?.reply))
                if (string.IsNullOrEmpty(TokenReply?.reply))
                    Reply = Replies[RandomGenerator.Next(Replies.Length-1)];
                else
                    Reply = TokenReply.reply;
            else
                Reply = PatternReply.reply;
            while (TweetSendedWithin15MinWindow >= 14)
                Thread.Sleep(30000);
            if (RandomGenerator.Next(100) >50)
                Tweet.FavoriteTweet(e.Tweet.Id);
            Thread.Sleep(RandomGenerator.Next(5000, 30000));
            var textToPublish = string.Format("@{0} {1}", e.Tweet.CreatedBy.ScreenName, Reply);
            var tweet = Tweet.PublishTweetInReplyTo(textToPublish, e.Tweet.Id);
            TweetSendedWithin15MinWindow++;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            KeywordStream.StopStream();
        }
    }
}
