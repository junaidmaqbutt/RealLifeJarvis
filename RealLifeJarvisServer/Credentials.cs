using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Core.Credentials;
using Tweetinvi.Models;

namespace RealLifeJarvisServer
{
    public class Credentials
    {

        #region Credentials
        #region Real Life Jarvis
        private static string RLJConsumerKey { get { return "j9nWA3WG3E0a99OGnyiN8NByk"; } }
        private static string RLJConsumerSecret { get { return "vO2GOxvk2wJoEXoZvLFnLisUebLMizO2yHGVaZulmfELwKnUsG"; } }
        private static string RLJAccessToken { get { return "4521360855-mwzendOAZKjxRReX5yKfnXNRiOIxnid4qqGU9Fr"; } }
        private static string RLJAccessSecret { get { return "KTmui7TsirLagki23mfhXzkI50beEBXXfXPOr71rEZhUe"; } }

        #endregion
        #region Talk To Me Jarvis
        private static string TTMJConsumerKey { get { return "Tpy1VvEVNIPFY6pA1MzxJqBel"; } }
        private static string TTMJConsumerSecret { get { return "3cfiO7ZEwzbAPgiOftyonXL6v8l2DX30cOiACVdtAndngugG9R"; } }
        private static string TTMJAccessToken { get { return "4521360855-ZIknQUJTkCsT4V1Xh0QxnNKpY67joyaUsYKsO4D"; } }
        private static string TTMJAccessSecret { get { return "0aJOWW5Xk2t6qnWshv6P6QH7ciJ4xer8U7yseUHTmQpvh"; } }

        #endregion
        #region Check Stream Jarvis 

        #endregion
        #endregion

        public static ITwitterCredentials RandomCredential
        {
            get
            {
                return Auth.CreateCredentials(RLJConsumerKey, RLJConsumerSecret, RLJAccessToken, RLJAccessSecret);
            }
        }
    }
}
