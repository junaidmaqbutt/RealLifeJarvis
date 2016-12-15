using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using VAAIVA.Database;

namespace VAAIVA.Corpse.Managment
{
    //************************************************
    //File Version 1.0 Basic Functionality Added !
    // Version 1.1 Added Static Replies Function Based on ReplyCode
    //************************************************
    public class ChatBot
    {
        internal static string COMMANDS_WEIGHT = "0.12";
        internal static string TOKEN_WEIGHT = "0.08";
        internal static string PATTERN_WEIGHT = "0.12";
        private string TableName;
        private bool isDBC = false;
        private DBConnect database;
        private List<string>[] CorpseData;
        private List<string> UsedReplies;
        internal string LastReply="";
        private int WarningCount = 0;
        internal bool isTrainingModeEnabled = false;
        public SystemType Type;

        /// <summary>
        /// Chatbot System. Can be used widly as a question-answering tool or just to animate a corpse data to find usefull information
        /// </summary>
        /// <param name="isNetAvailable">if true this function will fetch the data from the server and use that as training data for the chatbot</param>
        /// <param name="BotType">The type of bot from the SystemType enumerator. Each type has its own features.</param>
        /// <param name="isNULL">if false the chatbot will be initialized with the pre-learned data otherwise it will have no previous learning and will be completly null</param>
        public ChatBot(SystemType BotType,bool isNULL,bool isNetAvailable)
        {
            UsedReplies = new List<string>();
            string FilePath = System.Environment.CurrentDirectory + "\\SpecialTable";
            string FilePath1 = System.Environment.CurrentDirectory + "\\WordTable";
            Type = BotType;
            if (BotType == SystemType.ONE_WORD)
            {
                return;
            }
            else if (BotType == SystemType.TOKEN)
            {
                TableName = "Tokens";
                if (!isNULL)
                    if (isNetAvailable){
                        isDBC = true;
                        database = new DBConnect("vaaiva.com", "vaaiva_software", "vaaiva_soft", "VaaivaULT8");
                        List<string> temp = new List<string>();
                        temp.Add("PatternsVersion");
                        temp.Add("TokensVersion");
                        temp = database.Select("SoftwareDetails", "1", temp);
                        if (System.IO.File.Exists("0010"))
                        {
                            string[] line = System.IO.File.ReadAllLines("0010");
                            if (line[0] == temp[0] && line[1] == temp[1])
                                if (System.IO.File.Exists(FilePath1))
                                    CorpseData = (List<string>[])DeSerializeObject(FilePath1);
                                else
                                {
                                    CorpseData = database.SelectAll(TableName);
                                    SerializeObject(FilePath1, CorpseData);
                                }
                            else
                            {
                                CorpseData = database.SelectAll(TableName);
                                SerializeObject(FilePath1, CorpseData);
                            }
                        }
                        else
                        {
                            CorpseData = database.SelectAll(TableName);
                            SerializeObject(FilePath1, CorpseData);
                        }
                    }else
                        if (System.IO.File.Exists(FilePath1))
                            CorpseData = (List<string>[])DeSerializeObject(FilePath1);
                        else
                        {
                            CorpseData = new List<string>[3];
                            CorpseData[0] = new List<string>();
                            CorpseData[1] = new List<string>();
                            CorpseData[2] = new List<string>();
                        }
                else
                {
                    CorpseData = new List<string>[3];
                    CorpseData[0] = new List<string>();
                    CorpseData[1] = new List<string>();
                    CorpseData[2] = new List<string>();
                }
                return;
            }
            else if (BotType == SystemType.PATTERN)
            {
                TableName = "Patterns";
                if (!isNULL)
                    if (isNetAvailable){
                        isDBC = true;
                        database = new DBConnect("vaaiva.com", "vaaiva_software", "vaaiva_soft", "VaaivaULT8");
                        List<string> temp = new List<string>();
                        temp.Add("PatternsVersion");
                        temp.Add("TokensVersion");
                        temp = database.Select("SoftwareDetails", "1", temp);
                        if (System.IO.File.Exists("0010"))
                        {
                            string[] line = System.IO.File.ReadAllLines("0010");
                            if (line[0] == temp[0] && line[1] == temp[1])
                                if (System.IO.File.Exists(FilePath))
                                    CorpseData = (List<string>[])DeSerializeObject(FilePath);
                                else
                                {
                                    CorpseData = database.SelectAll(TableName);
                                    SerializeObject(FilePath, CorpseData);
                                }
                            else
                            {
                                CorpseData = database.SelectAll(TableName);
                                SerializeObject(FilePath, CorpseData);
                            }
                        }
                        else
                        {
                            CorpseData = database.SelectAll(TableName);
                            SerializeObject(FilePath, CorpseData);
                        }
                    }else
                        if (System.IO.File.Exists(FilePath))
                            CorpseData = (List<string>[])DeSerializeObject(FilePath);
                        else
                        {
                            CorpseData = new List<string>[3];
                            CorpseData[0] = new List<string>();
                            CorpseData[1] = new List<string>();
                            CorpseData[2] = new List<string>();
                        }
                else
                {
                    CorpseData = new List<string>[3];
                    CorpseData[0] = new List<string>();
                    CorpseData[1] = new List<string>();
                    CorpseData[2] = new List<string>();
                }
            }
            else if (BotType == SystemType.COMMAND)
            {
                CorpseData = new List<string>[4];
                CorpseData[0] = new List<string>();
                CorpseData[1] = new List<string>();
                CorpseData[2] = new List<string>();
                CorpseData[3] = new List<string>();
                if (!isNULL)
                    trainfromCommands();
            }
        }

        #region *** Corpse Data Managment System ***
        private bool doesExist(string word)
        {
            int index = CorpseData[0].FindIndex(X => X == word);
            if (index == -1) return false;
            return true;
        }
        private List<string> Select(string word)
        {
            List<string> list = new List<string>();
            int index = CorpseData[0].FindIndex(X => X == word);
            list.Add(CorpseData[0][index]);
            list.Add(CorpseData[1][index]);
            list.Add(CorpseData[2][index]);
            return list;
        }
        private void Update(string word, string answer,string weight)
        {
            int index = CorpseData[0].FindIndex(X => X == word);
            CorpseData[1][index] = answer;
            CorpseData[2][index] = weight;
        }
        private void Insert(string word, string answer, string weight)
        {
            CorpseData[0].Add(word);
            CorpseData[1].Add(answer);
            CorpseData[2].Add(weight);
        }
        private void Delete(string Token)
        {
            int index = CorpseData[0].FindIndex(X => X == Token);
            if (index == -1) return;
            CorpseData[0].RemoveAt(index);
            CorpseData[1].RemoveAt(index);
            CorpseData[2].RemoveAt(index);
            if (CorpseData.Length == 4)
                CorpseData[3].RemoveAt(index);
        }
        public void deleteReply(string line, string answer)
        {
            string[] tokens = line.Split(' ');
            foreach (string token in tokens)
            {
                string T = token;
                if (T == "") continue;
                string word;
                string[] answers;
                string[] weights;
                int index;
                if (Type == SystemType.PATTERN)
                    index = getSpecialReplyIndex(line);
                else
                    index = CorpseData[0].FindIndex(X => X == T);
                if (index == -1) continue;
                word = CorpseData[0][index];
                answers = CorpseData[1][index].Split(',');
                weights = CorpseData[2][index].Split(',');
                string newAns = "";
                string newWei = "";
                for (int x = 0; x < answers.Length; x++)
                {
                    if (answers[x] == answer)
                    {
                        continue;
                    }
                    else
                    {
                        newAns += " , " + answers[x];
                        newWei += " , " + weights[x];
                    }
                }
                if (newAns == "")
                {
                    Delete(word);
                    if (isDBC)
                        database.Delete(TableName, "\"" + word + "\"");
                    continue;
                }
                newAns = newAns.Remove(0, 3);
                newWei = newWei.Remove(0, 3);
                Delete(word);
                database.Delete(TableName, "\"" + word + "\"");
                database.Insert(TableName, "Word,Answers,Weights", "\"" + word + "\" , \"" + newAns + "\", \"" + newWei + "\"");
                Insert(word, newAns,newWei);
                if (Type == SystemType.PATTERN) break;
            }
            
        }
        public void DeleteAll()
        {
            if (isDBC)
                database.Delete(TableName);
        }
        public int DBCount()
        {
            if (isDBC)
                return database.Count(TableName);
            else return 0;
        }
        public void close()
        {
            database.CloseConnection();
        }
        #endregion

        #region *** Training Managment System ***
        private void trainfromCommands()
        {
            string[] Lines = RealLifeJarvisServer.Properties.Resources.Commands.Split('\n');
            foreach(string line in Lines){
                if (line.StartsWith("#")) continue;
                if (line.StartsWith("/")) continue;
                if (line == "" || line.Replace(" ", "") == "") continue;
                trainFromQA(line.Split('|')[0], line.Split('|')[1].Replace("\r",""),COMMANDS_WEIGHT);
            }
            string[] Keywords = RealLifeJarvisServer.Properties.Resources.Commands.Substring(RealLifeJarvisServer.Properties.Resources.Commands.IndexOf("//@@StartKeyword"), RealLifeJarvisServer.Properties.Resources.Commands.IndexOf("//@@EndKeyword")).Split('\n');
            foreach (string Keyword in Keywords)
            {
                if (Keyword.StartsWith("//@@")) continue;
                foreach (string word in Keyword.Replace("/", "").Split(' '))
                    if (word.Replace(" ", "").Replace("\r", "") == "")
                        continue;
                    else
                        CorpseData[3].Add(word.Replace("\r", ""));
            }
            
        }
        public void TrainFromPanelCommands(string PanelName)
        {
            if (RealLifeJarvisServer.Properties.Resources.PanelCommands.IndexOf("//@@Start" + PanelName) == -1) return;
            if (RealLifeJarvisServer.Properties.Resources.PanelCommands.IndexOf("//@@End" + PanelName) == -1) return;
            string temp = RealLifeJarvisServer.Properties.Resources.PanelCommands.Substring(RealLifeJarvisServer.Properties.Resources.PanelCommands.IndexOf("//@@Start" + PanelName));
            string[] Lines = temp.Substring(0, temp.IndexOf("//@@End" + PanelName)).Split('\n');
            foreach (string line in Lines)
            {
                if (line.StartsWith("#")) continue;
                if (line.StartsWith("/")) continue;
                if (line == "" || line.Replace(" ", "") == "") continue;
                trainFromQA(line.Split('|')[0], line.Split('|')[1].Replace("\r", ""), COMMANDS_WEIGHT);
            }
            temp = RealLifeJarvisServer.Properties.Resources.PanelCommands.Substring(RealLifeJarvisServer.Properties.Resources.PanelCommands.IndexOf("//@@StartKeyword" + PanelName));
            string[] Keywords = temp.Substring(0, temp.IndexOf("//@@EndKeyword" + PanelName)).Split('\n');
            foreach (string Keyword in Keywords)
            {
                if (Keyword.StartsWith("//@@")) continue;
                foreach (string word in Keyword.Replace("/", "").Split(' '))
                    if (word.Replace(" ", "").Replace("\r", "") == "")
                        continue;
                    else
                        CorpseData[3].Add(word.Replace("\r", ""));
            }
        }
        public bool trainfromObjectList<T>(List<T> list)
        {
            for (int x = 0; x < list.Count; x++)
                if (list[x].ToString().Contains("#"))
                {
                    if (Type == SystemType.PATTERN)
                        trainFromQA(list[x].ToString().Split('#')[0], list[x].ToString().Split('#')[1].Split('.')[0], (float.Parse(PATTERN_WEIGHT) * list[x].ToString().Split('#')[0].Split(' ').Length).ToString());
                    else
                        trainFromQA(list[x].ToString().Split('#')[0], list[x].ToString().Split('#')[1].Split('.')[0], TOKEN_WEIGHT);
                }
            return true;
        }
        public bool trainFromList<T>(List<T>[] list)
        {
            for (int x = 0; x < list[0].Count; x++)
                trainFromQA(list[0][x].ToString(), list[1][x].ToString(),list[2][x].ToString());
            return true;
        }
        public void trainFromQA(string line, string answer,string weight)
        {
            line = line.ToLower();
            line = parseInput(line);
            answer = answer.Replace("\"", "").ToLower();
            if (line.Split(' ').Length < 1) return;
            if (Type == SystemType.PATTERN ) trainDataSpecial(line, answer,weight);
            else if (Type == SystemType.TOKEN || Type == SystemType.COMMAND)
            {
                foreach (string token in line.Split(' '))
                {
                    trainData(token, answer,weight);
                }
            }
        }
        private bool trainData(string word, string answer,string weight)
        {
            if (answer == null || answer.Replace(",","").Trim() == "") return false;
            if (word == null || word.Trim() == "") return false;
            answer = answer.Replace(",", " ");
            if (doesExist(word))
            {
                List<string> list = Select(word);
                answer = answer.Replace("\"", "");

                if (("," + list[1] + ",").ToLower().Contains(","+ answer.ToLower() + ","))
                {
                    return true;
                }
                answer = list[1] + "," + answer;
                weight = list[2] + "," + weight;
                try
                {
                    if (isDBC)
                        database.Update(TableName, word, answer,weight);
                    Update(word, answer,weight);
                    return true;
                }
                catch (Exception e) { return false; }

            }
            else
            {
                try
                {
                    if (isDBC)
                        database.Insert(TableName, "Word,Answers,Weights", "\"" + word + "\"" + "," + "\"" + answer + "\"" + "," + "\"" + weight + "\"");
                    Insert(word , answer,weight);
                    return true;
                }
                catch (Exception e) { return false; }
            }
        }
        private bool trainDataSpecial(String word, string answer,string weight)
        {
            answer = answer.Replace(",", " ");
            if (word == null || word.Trim() == "") return false;
            if (answer == null || answer.Trim() == "") return false;
            if (doesExist(word))
            {
                List<string> list = Select(word);
                answer = answer.Replace("\"", "");
                if (list[1].ToLower().Contains(answer.ToLower()))
                {
                    return true;
                }
                answer = list[1] + "," + answer;
                try
                {
                    if (isDBC)
                        database.Update(TableName, word, answer,weight);
                    Update(word, answer,weight);
                    return true;
                }
                catch (Exception e) { return false; }

            }
            else
            {
                try
                {
                    if (isDBC)
                        database.Insert(TableName, "Word,Answers,Weights", "\"" + word + "\"" + "," + "\"" + answer + "\"" + "," + "\"" + weight + "\"");
                    Insert(word , answer,weight);
                    return true;
                }
                catch (Exception e) { return false; }
            }
        }
        public void UpdateWeights(String Question, string Answer, string OldAnswer)
        {
            Question = Question.ToLower().Trim();
            if (Type == SystemType.TOKEN)
            {
                Question = removePunctuations(Question);
                string[] Questions = Question.Split(' ');
                int AnswerIndex;
                float Weight;
                foreach (string Token in Questions)
                {
                    if (Token == "") continue;
                    int index = CorpseData[0].IndexOf(Token);
                    List<string> Answers = new List<string>(CorpseData[1][index].Split(','));
                    List<string> Weights = new List<string>(CorpseData[2][index].Split(','));
                    AnswerIndex = Answers.IndexOf(OldAnswer);
                    if (AnswerIndex != -1)
                    {
                        Weight = float.Parse(Weights[AnswerIndex]) - (float)0.005;
                        Weights[AnswerIndex] = Weight + "";
                    }
                    AnswerIndex = Answers.IndexOf(Answer);
                    if (AnswerIndex == -1)
                    {
                        trainData(Token, Answer, TOKEN_WEIGHT);
                        Answers = new List<string>(CorpseData[1][index].Split(','));
                        Weights = new List<string>(CorpseData[2][index].Split(','));
                        index = CorpseData[0].IndexOf(Token);
                        CorpseData[1][index] = String.Join(",", Answers);
                        AnswerIndex = Answers.IndexOf(Answer);
                    }
                    Weight = float.Parse(Weights[AnswerIndex]) + (float)0.01;
                    Weights[AnswerIndex] = Weight + "";
                    CorpseData[2][index] = String.Join(",", Weights);
                }
            }
            else
            {
                int index = getSpecialReplyIndex(Question);
                List<string> Weights = new List<string>(CorpseData[2][index].Split(','));
                Weights[0] = (float.Parse(Weights[0]) - (float)0.015) + "";
                CorpseData[2][index] = String.Join(",", Weights);
            }
        }
        #endregion

        #region *** Reply Managment System ***
        public Reply getReply(string input)
        {
            input = input.ToLower().Trim();
            if (input == "why do you keep asking me the same question" && Type != SystemType.COMMAND)
            {
                string[] reply = CorpseData[1][new Random().Next(0, CorpseData[1].Count)].Split(',');
                return new Reply("I dont know maybe i was stuck ... Try Asking something different",float.Parse("0.6"));
            }
            Reply Ans= new Reply();
            if (Type == SystemType.TOKEN)
                Ans= PatternMatchReply(input);
            else if (Type == SystemType.COMMAND)
                Ans = PatternMatchCommandReply(input);
            else if (Type == SystemType.PATTERN )
            {
                Reply R = StringMatchReply(input);
                if (LastReply == "" && R!=null) Ans = R;
                if (R != null && R.reply == LastReply && Type != SystemType.COMMAND)
                {
                    if (WarningCount >= 2 && !isTrainingModeEnabled)
                    {
                        Ans = new Reply("Why do you keep asking me the same question ?", float.Parse("0.4"));
                        WarningCount = 0;
                    }
                    else
                        WarningCount++;
                }
                else Ans = R;
            }
            else if (Type == SystemType.ONE_WORD)
                Ans = OneWordReply(input);
            if (Ans != null) LastReply = Ans.reply;
            if (Ans != null && input == Ans.reply) Ans = getReply(input);
            return Ans;
        }
        private Reply PatternMatchCommandReply(string input)
        {
            bool hasCommandKeyword = false;
            for (int x = 0; x < CorpseData[3].Count; x++)
                if (input.Contains(CorpseData[3][x]))
                    hasCommandKeyword = true;
            if (!hasCommandKeyword) return null;
            return PatternMatchReply(input);
        }
        private Reply PatternMatchReply(string input) 
        {
            ReplyList allReplyList = new ReplyList();
            if (CorpseData.Length == 0) return new Reply();
            input = removePunctuations(input);
            foreach (string token in input.Split(' '))
            {   
                int index = CorpseData[0].FindIndex(X => X == token);
                if (index == -1) continue;
                allReplyList.parseReplyList(CorpseData[1][index].Split(','), CorpseData[2][index].Split(','), Type);
            }
            if (allReplyList.isEmpty) return null;
            List<Reply> Replies = ReplyList.findBestReply(allReplyList);
            if (Replies.Count == 0)
                return null;
            else if (Replies.Count == 1){
                if (LastReply == "") return Replies[0];
                Reply R = Replies[0];
                if (R.reply == LastReply && Type != SystemType.COMMAND) {
                    if (WarningCount >= 2 && !isTrainingModeEnabled)
                    {
                        WarningCount = 0;
                        return new Reply("Why do you keep asking me the same question ?", float.Parse("0.4"));
                    }
                    else
                    {
                        WarningCount++;
                        return R;
                    }                        
                }
                else return R;
            }
            else
            {
                foreach (Reply R in Replies)
                {
                    if (UsedReplies.FindIndex(UR => UR==R.reply) == -1)
                    {
                        UsedReplies.Add(R.reply);
                        return R;
                    }
                }
                foreach (Reply R in Replies)
                {
                    UsedReplies.Remove(R.reply);
                }
                int rand = new Random().Next(0, Replies.Count);
                UsedReplies.Add(Replies[rand].reply);
                return Replies[rand];
            }
        }
        private Reply StringMatchReply(String input) 
        {
            int specialCase = getSpecialReplyIndex(input);
            if (specialCase != -1)
            {
                return new Reply(CorpseData[1][specialCase].Split(',')[0], float.Parse(CorpseData[2][specialCase].Split(',')[0]), CorpseData[0][specialCase].Split(' ').Length);
            }
            return null;
        }
        private Reply OneWordReply(string input)
        {
            Reply reply=new Reply();
            if (input == "") reply.reply="My brain just went Up and Down seeing what you have typed.";
            if (input == "hi") reply.reply="Hi !!! How Are you";
            if (input == "helo") reply.reply="Hi !!! How Are you";
            if (input == "hello") reply.reply="Hi !!! How Are you";
            if (input == "bye") reply.reply="Bye Bye Sir.";
            if (input == "good") reply.reply="Il say its Great";
            if (input == "fine") reply.reply="Good !!!";
            if (input == "great") reply.reply="Yes it is Great";
            if (input == "awesome") reply.reply="Superb !";
            if (input == "super") reply.reply="WOW !";
            if (input == "superb") reply.reply="WOW !";
            if (input == "sad") reply.reply="and why is that ?";
            if (input == "happy") reply.reply="wow thats awesome";
            if (input == "lol") reply.reply="you seem quite happy";
            if (input == "ok") reply.reply="Ok it is.";
            if (input.Contains("lmao")) reply.reply="You do know that contains a bad word in it.";
            if (input.Contains("haha")) reply.reply="You seem quite happy.";
            if (input.Contains("thank")) reply.reply="you are welcome sir.";
            if (reply.reply !=null) reply.weight = (float)1.0;
            else return null;
            return reply;
            
        }
        private int getSpecialReplyIndex(string line)
        {
            List<int> found = new List<int>();
            for (int x = 0; x < CorpseData[0].Count; x++)
                if (line.Contains(CorpseData[0][x]))
                    found.Add(x);
            int temp = 0, HighIndex=0;
            foreach (int x in found)
            {
                if (CorpseData[0][x].Length > temp)
                {
                    temp = CorpseData[0][x].Length;
                    HighIndex = x;
                }
            }
            if (found.Count == 0) return -1;
            return HighIndex;
        }
        public Reply getStaticReply(ReplyType type)
        {
            Reply reply = new Reply(null, 0);
            switch (type)
            {
                case ReplyType.NO_REPLY:
                    reply.reply = "Could you repeat that";
                    break;
                case ReplyType.SAME_QUESTION:
                    reply.reply = "Try asking me a different question";
                    break;
            }
            return reply;
        }
        #endregion

        #region *** Input Managment Functions ***
        public static string parseInput(string line)
        {

            line = line.ToLower();

            line = removeStopWords(line);
            line = removePunctuations(line);

            line = "." + line + ".";
            while (line.Contains(". ") || line.Contains(" .") || line.Contains("  "))
            {
                line = line.Replace(". ", ".");
                line = line.Replace(" .", ".");
                line = line.Replace("  ", " ");
            }
            line = line.Replace(".", "");

            return line;
        }
        private static string removeStopWords(string line)
        {
            /*
            line = line.Replace(" a ", " ");
            line = line.Replace(" an ", " ");
            line = line.Replace(" be ", " ");
            line = line.Replace(" by ", " ");
            line = line.Replace(" for ", " ");
            line = line.Replace(" of ", " ");
            line = line.Replace(" on ", " ");
            line = line.Replace(" or ", " ");
            line = line.Replace(" the ", " ");
            */
            return line;
        }
        internal static string removePunctuations(string line)
        {
            line = line.Replace("`", "");
            line = line.Replace("~", "");
            line = line.Replace("!", "");
            line = line.Replace("@", "");
            line = line.Replace("#", "");
            line = line.Replace("$", "");
            line = line.Replace("%", "");
            line = line.Replace("^", "");
            line = line.Replace("&", "");
            line = line.Replace("*", "");
            line = line.Replace("(", "");
            line = line.Replace(")", "");
            line = line.Replace("-", "");
            line = line.Replace("_", "");
            line = line.Replace("=", "");
            line = line.Replace("+", "");
            line = line.Replace("[", "");
            line = line.Replace("{", "");
            line = line.Replace("]", "");
            line = line.Replace("}", "");
            line = line.Replace(";", "");
            line = line.Replace(":", "");
            line = line.Replace("'", "");
            line = line.Replace("\"", "");
            line = line.Replace(",", "");
            line = line.Replace("<", "");
            line = line.Replace(".", "");
            line = line.Replace(">", "");
            line = line.Replace("/", "");
            line = line.Replace("?", "");
            line = line.Replace("\\", "");
            line = line.Replace("|", "");

            return line;
        }
        #endregion

        #region *** Object To File ***
        private void SerializeObject(string filename, object obj)
        {
            System.IO.Stream stream = System.IO.File.Open(filename, System.IO.FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, obj);
            stream.Close();
            List<string> temp = new List<string>();
            temp.Add("PatternsVersion");
            temp.Add("TokensVersion");
            temp = database.Select("SoftwareDetails", "1", temp);
            System.IO.File.WriteAllText("0010", temp[0] + "\n" + temp[1]);
        }
        private Object DeSerializeObject(string filename)
        {
            Object obj;
            System.IO.Stream stream = System.IO.File.Open(filename, System.IO.FileMode.Open);
            BinaryFormatter bFormatter = new BinaryFormatter();
            obj = (Object)bFormatter.Deserialize(stream);
            stream.Close();
            return obj;
        }
        #endregion
        private string getNames(string input)
        {
            string[] wordsInput = parseInput(input.ToLower()).Split(' ');
            string wordList = System.IO.File.ReadAllText(@"C:\Users\Junaid\Desktop\AllWords.txt");
            wordList = "\n" + wordList.ToLower() + "\n";
            string output = null;
            bool last = true;
            foreach (string word in wordsInput)
            {
                if (!wordList.Contains("\n" + word + "\n"))
                {
                    if (last == true)
                        output += word + " ";
                    else output += "\n" + word + " ";
                    last = true;
                }
                else if (output != null) last = false;
            }
            if (output == null) output = "None";
            return output;
        }
    }

    public enum SystemType 
    { 
        PATTERN, 
        TOKEN,
        ONE_WORD,
        COMMAND
    }
    public enum ReplyType : int
    {
        NO_REPLY =0,
        SAME_QUESTION =1
    }

    #region *** REPLY DATA STRUCTURE ***
    public class ReplyList
    {
        private List<Reply> list;

        public bool isEmpty
        {
            get
            {
                if (list.Count > 0) return false;
                else return true;
            }
        }

        public ReplyList() { list = new List<Reply>(); }

        public void Add(Reply T)
        {
            int x = list.FindIndex(X => X.reply == T.reply);
            if (x == -1) list.Add(T);
            else
            {
                list[x].weight += T.weight - ((list[x].NumberOfItemsCombined % 10) /100);
                list[x].NumberOfItemsCombined++;
                list[x].weight += ((list[x].NumberOfItemsCombined % 10) / 100);
            }
        }

        public void parseReplyList(string[] replies,string[] weights,SystemType type)
        {
            if (type != SystemType.COMMAND)
            {
                float sum ;
                sum = float.Parse(weights[0]) * weights.Length;
                for (int x = 0; x < replies.Length; x++)
                {
                    Add(new Reply(replies[x], float.Parse(weights[x]) / sum));
                }
                    
            }
            else
                for (int x = 0; x < replies.Length; x++)
                    Add(new Reply(replies[x], float.Parse(weights[x])));
        }

        public static List<Reply> findBestReply(ReplyList input)
        {
            float weight = -1;
            List<Reply> list = new List<Reply>();
            foreach (Reply R in input.list)
            {
                //Check equal or greater weights
                if (weight == R.weight)
                {
                    list.Add(R);
                }
                if (weight < R.weight)
                {
                    list.Clear();
                    list.Add(R);
                    weight = R.weight;
                }
            }

            return list;
        }
    }
    public class Reply
    {
        //Members
        private string _reply;
        private float _weight;
        private int _CombinedWeightOfItems;
        public string reply
        {
            get { return _reply; }
            set { _reply = value; }
        }
        public float weight
        {
            get { return _weight; }
            set { _weight = value; }
        }
        public int NumberOfItemsCombined
        {
            get { return _CombinedWeightOfItems; }
            set { _CombinedWeightOfItems = value; }
        }
        //Constructors
        public Reply()
        {
            reply = null;
            weight = 0;
        }
        public Reply(string R, float W,int N=1)
        {
            reply = R;
            weight = W;
            NumberOfItemsCombined = N;
        }

    }

    #endregion

}
