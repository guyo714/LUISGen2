using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;


namespace LUISGen2
{
    class Program
    {
        class IntentClass
        {
            public List<Dictionary<string, string>> intents;
        }

        private enum EntityType
        {
            entities,
            composites,
            closedLists,
            patternAnyEntities,
            regex_entities,
            prebuildEntities,
        }

        static void Main(string[] args)
        {

            LUISObjectGen luisObjectGen = null;

            void makeupArgs()
            {
                args = new string[5];
                args[0] = "/Users/E670206/Documents/Chatbot/LuisJson/RSOChatbotDevLuisV2.json";
                //args[0] = "/Users/E670206/Documents/Chatbot/LuisJson/RSOChatbotDevLuisV2-OLD.json";
                args[1] = "-cs";
                args[2] = "RSOChatbotDevLuisV2";
                args[3] = "-o";
                args[4] = "/Users/E670206/Documents/Chatbot/LuisJson";
            }

            makeupArgs();

            string jsonFileName = string.Empty;
            string outFileName = string.Empty;
            string outPath = string.Empty;

            for (int index = 0; index < args.Length; index++)
            {
                string argValue = args[index].ToLower();

                if (jsonFileName == string.Empty)
                {
                    jsonFileName = argValue;
                }
                else
                {
                    switch (argValue)
                    {
                        case "-cs":
                            outFileName = args[++index];
                            break;
                        case "-o":
                            outPath = args[++index] + "/" + outFileName +  ".cs";
                            break;
                        default:
                            break;
                    }
                }
            }

            StreamReader reader = new StreamReader(jsonFileName);
            dynamic luisObject = JsonConvert.DeserializeObject(reader.ReadToEnd());

            luisObjectGen = new LUISObjectGen(luisObject);

            luisObjectGen.setupStreamWriter(outPath);

            luisObjectGen.luisIntentGen.FlushFileHeader();
            luisObjectGen.luisIntentGen.generate(luisObject);
            //LUISIntentGen.Instance.FlushFileHeader();
            //LUISIntentGen.Instance.generate(luisObject);

            luisObjectGen.luisEntityGen.generate(luisObject);

            luisObjectGen.luisSentiumentGen.generate();
            //LUISSentimentGen.Instance.generate();

            //LUISEntityGen.Instance.FlushFileTrailer();
            luisObjectGen.luisEntityGen.FlushFileTrailer();

            luisObjectGen.FlushBuffer();

        }
    }
}

