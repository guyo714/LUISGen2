using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;

namespace LUISGen2
{
    public class LUISObjectGen
    {

        public LUISObjectGen()
        {
        }

        public LUISObjectGen(dynamic luisObject)
        {
            version = luisObject.luis_schema_version.Value;
        }

        private LUISEntityGen _luisEntityGen = null;

        public LUISEntityGen luisEntityGen
        {
            get
            {
                if (_luisEntityGen == null)
                {
                    if (String.Compare(version, version7) == 0)
                    {
                        _luisEntityGen = new LUISEntityGen7(indent, writer);
                    }
                    else
                    {
                        _luisEntityGen = new LUISEntityGen(indent, writer);
                    }
                }

                return _luisEntityGen;
            }
        }

        private LUISIntentGen _luisIntentGen = null;

        public LUISIntentGen luisIntentGen
        {
            get
            {
                if (_luisIntentGen == null)
                {
                    _luisIntentGen = new LUISIntentGen(indent, writer);
                }

                return _luisIntentGen;
            }
        }

        private LUISSentimentGen _luisSentiumentGen = null;

        public LUISSentimentGen luisSentiumentGen
        {
            get
            {
                if (_luisSentiumentGen == null)
                {
                    _luisSentiumentGen = new LUISSentimentGen(indent, writer);
                }

                return _luisSentiumentGen;
            }
        }

        public static string version4 = "4.0.0";
        public static string version7 = "7.0.0";

        protected string version;

        protected Indent indent = Indent.Instance;

        protected StreamWriter writer = null;

        public void setupStreamWriter(string outPath)
        {
            writer = new StreamWriter(outPath, false);
        }

        public void WriteLine(string buffer)
        {
            writer.WriteLine(buffer);
            Console.WriteLine(buffer);
        }

        public void WriteEmptyLine()
        {
            string buffer = "";
            writer.WriteLine(buffer);
            Console.WriteLine(buffer);
        }

        public void FlushBuffer()
        {
            writer.Flush();
        }

        public void FlushFileHeader()
        {
            indent.reset();

            WriteLine(indent + "using System;");
            WriteLine(indent + "using System.Collections.Generic;");
            WriteLine(indent + "using System.Linq;");
            WriteLine(indent + "using Microsoft.Bot.Builder;");
            WriteLine(indent + "using Microsoft.Bot.Builder.AI.Luis;");
            WriteEmptyLine();

            WriteLine(indent + "#pragma warning disable SA1137");
            WriteEmptyLine();

            WriteLine(indent + "namespace LUIS");
            WriteLine(indent + "{");
        }

        public void generateLUISModel()
        {
            WriteLine(++indent + "public class LUISModel");
            WriteLine(indent + "{");

            generateRoleEntityList();

            WriteLine(++indent + "public bool validModel { get; set; }");
            WriteLine(indent + "public string utterance { get; set; }");
            WriteLine(indent + "public string originalUtterance { get; set; }");
            WriteLine(indent + "public IntentType topIntent { get; set; }");
            WriteLine(indent + "public Dictionary<Entity, object> entities = new Dictionary<Entity, object>();");
            WriteLine(indent + "public Sentiment sentiment { get; set; }");
            WriteEmptyLine();

            WriteLine(indent + "public T[] entity<T>(Entity type)");
            WriteLine(indent + "{");
            WriteLine(++indent + "foreach (KeyValuePair<Entity, object> keyValue in entities)");
            WriteLine(indent + "{");
            WriteLine(++indent + "if (keyValue.Key == type)");
            WriteLine(indent + "{");
            WriteLine(++indent + "return (T[])keyValue.Value;");
            WriteLine(--indent + "}");
            WriteEmptyLine();
            WriteLine(indent + "if (keyValue.Value.GetType().Equals(typeof(RoleEntityType[])))");
            WriteLine(indent + "{");
            WriteLine(++indent + "RoleEntityType[] roleEntityTypes = keyValue.Value as RoleEntityType[];");
            WriteLine(indent + "foreach (RoleEntityType roleEntityType in roleEntityTypes)");
            WriteLine(indent + "{");
            WriteLine(++indent + "if (roleEntityType.roleType == type)");
            WriteLine(indent + "{");
            WriteLine(++indent + "return roleEntityTypes as T[];");
            WriteLine(--indent + "}");
            WriteLine(--indent + "}");
            WriteLine(--indent + "}");
            WriteLine(--indent + "}");
            WriteEmptyLine();
            WriteLine(indent + "return default(T[]);");
            WriteLine(--indent + "}");

            WriteEmptyLine();
            WriteLine(indent + "public Dictionary<string, string> words = null;");
            WriteEmptyLine();
            WriteLine(indent + "public string originalWord(string word)");
            WriteLine(indent + "{");
            WriteLine(++indent + "if (words == null)");
            WriteLine(indent + "{");
            WriteLine(++indent + "return null;");
            WriteLine(--indent + "}");
            WriteEmptyLine();
            WriteLine(indent + "string upperWord = word.ToUpper();");
            WriteLine(indent + "if (words.ContainsKey(upperWord))");
            WriteLine(++indent + "{");
            WriteLine(++indent + "return words[upperWord];");
            WriteLine(--indent + "}");
            WriteEmptyLine();
            WriteLine(indent + "return null;");
            WriteLine(--indent + "}");
    
            WriteLine(--indent + "}");
        }

        public void FlushFileTrailer()
        {
            generateLUISModel();
            indent.reset();
            WriteLine(indent + "}");
        }

        public virtual void generate(dynamic luisObject)
        {
            throw new NotImplementedException();
        }
        public virtual void generateRoleEntityList()
        {
            throw new NotImplementedException();
        }

    }

    /*
     * Intent
     */
    public class LUISIntentGen : LUISObjectGen
    {
        public LUISIntentGen(Indent indent, StreamWriter writer) : base()
        {
            base.indent = indent;
            base.writer = writer;
        }

        public override void generate(dynamic luisObject)
        {
            dynamic intents = luisObject.intents;

            if (intents.Count == 0)
            {
                return;
            }

            WriteLine(++indent + "public enum Intent");
            WriteLine(indent + "{");
            indent++;

            foreach (dynamic intent in intents)
            {
                WriteLine(indent + (string)intent.name + ",");
            }
            WriteLine(--indent + "}");
            --indent;
            WriteEmptyLine();

            /*
             *
             *  public class IntentType
             *  {
             *      public Intent intent;
             *      public float score;
             *  }
             */

            WriteLine(++indent + "public class IntentType");
            WriteLine(indent + "{");
            indent++;

            WriteLine(indent + "public Intent intent { get; set; }");
            WriteLine(indent + "public float score { get; set; }");

            WriteLine(--indent + "}");
            --indent;
            WriteEmptyLine();
        }

    }

    public class LUISEntityGen : LUISObjectGen
    {
        public LUISEntityGen(Indent indent, StreamWriter writer) : base()
        {
            base.indent = indent;
            base.writer = writer;

            entities = new Dictionary<string, dynamic>();
            entityNames = new SortedDictionary<string, List<string>>();
        }

        private enum EntityType
        {
            composites,
            closedLists,
            patternAnyEntities,
            regex_entities,
            prebuiltEntities,
            entities,
        }

        protected List<string> roleEntityTypeNames = new List<string>();

        protected Dictionary<string, dynamic> entities;
        protected SortedDictionary<string, List<string>> entityNames;

        //public LUISEntityGen()
        //{
        //    entities = new Dictionary<string, dynamic>();
        //    entityNames = new SortedDictionary<string, List<string>>();
        //}

        private void generateTypeClass()
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            // Generate Base Class
            /*
             *     public class EntityTypeBase
             *     {
             *          public Entity roleType { get; set; }
             *          public string value { get; set; }
             *      }
             *      
             */
            WriteLine(++indent + "public abstract class EntityTypeBase");
            WriteLine(indent + "{");
            indent++;
            WriteLine(indent + "public Entity type { get; set; }");
            WriteLine(indent + "public string value { get; set; }");
            WriteLine(indent-- + "}");
            indent--;
            WriteEmptyLine();

            // Generate SimpleEmtityType
            /*
             *      public class SimpleEntityType : EntityTypeBase
             *      {
             *      }
             */
            WriteLine(++indent + "public class SimpleEntityType : EntityTypeBase");
            WriteLine(indent + "{");
            WriteLine(indent + "}");
            indent--;
            WriteEmptyLine();

            // Generate Prebuild Entity Type with a typed object
            /*    public class PrebuildEntityType<T> : EntityTypeBase
             *    {
             *     public T typedObject { get; set; }
             *    } 
             */
            WriteLine(++indent + "public class PrebuildEntityType : EntityTypeBase");
            WriteLine(indent + "{");
            WriteLine(++indent + "public object typedObject { get; set; }");
            WriteLine(--indent + "}");
            indent--;
            WriteEmptyLine();

            // Generate Simple Entity Type with Role
            /*      public class RoleEntityType : EntityTypeBase
             *      {
             *      public Entity roleType { get; set; }
             *      }
             */
            WriteLine(++indent + "public class RoleEntityType : EntityTypeBase");
            WriteLine(indent + "{");
            indent++;
            WriteLine(indent + "public Entity roleType { get; set; }");
            WriteLine(indent-- + "}");
            indent--;
            WriteEmptyLine();

            // Generate Composite Entity Type
            /*
             *     public class CompositeEntityType : EntityTypeBase
             *     {
             *          public Dictionary<Entity, object> entities = new Dictionary<Entity, object>();
             *     }
             */     

            WriteLine(++indent + "public class CompositeEntityType : EntityTypeBase");
            WriteLine(indent + "{");
            WriteLine(++indent + "public Dictionary<Entity, object> entities = new Dictionary<Entity, object>();");
            WriteLine(indent-- + "}");
            indent--;
            WriteEmptyLine();
        }

        public override void generateRoleEntityList()
        {
            WriteLine(++indent + "public static List<Entity> RoleEntityTypes = new List<Entity>()");
            WriteLine(++indent + "{");

            ++indent;

            foreach (string name in roleEntityTypeNames)
            {
                WriteLine(indent + "Entity." + name + ",");
            }
            WriteLine(--indent + "};");
            WriteEmptyLine();
            --indent;
            --indent;

        }

        public virtual void generateBelongings(dynamic entity, List<string> names)
        {
            if (entity == null)
            {
                return;
            }

            foreach (dynamic belonging in entity)
            {
                string belongingString = (string)belonging;
                if (!entities.ContainsKey(belongingString))
                {
                    names.Add(belongingString);
                    entities[belongingString] = belonging;
                }
            }
        }

        public virtual void generateRoleBelongings(dynamic entity, List<string> names)
        {
            generateBelongings(entity, names);
        }

        public override void generate(dynamic luisObject)
        {

            foreach (EntityType entityType in Enum.GetValues(typeof(EntityType)))
            {
                dynamic luisEntities = luisObject[entityType.ToString()];
                foreach (dynamic entity in luisEntities)
                {
                    string name = entity.name;
                    List<string> names = new List<string>();

                    if (!entities.ContainsKey(name))
                    {
                        generateBelongings(entity.children, names);
                        generateRoleBelongings(entity.roles, names);

                        if (entity.roles != null && entity.roles.Count > 0)
                        {
                            roleEntityTypeNames.Add(name);
                        }

                        entityNames[name] = names.Count == 0 ? null : names;
                        entities[name] = entity;
                    }
                }
            }

            WriteLine(++indent + "public enum Entity");
            WriteLine(indent + "{");
            indent++;

            foreach (KeyValuePair<string, List<string>> entityName in entityNames)
            {
                WriteLine(indent + entityName.Key + ",");

                if (entityName.Value != null)
                {
                    entityName.Value.Sort();
                    indent++;
                    foreach (string subValue in entityName.Value)
                    {
                        WriteLine(indent + subValue + ",");
                    }
                    indent--;
                }
            }

            WriteLine(--indent + "}");
            --indent;
            WriteEmptyLine();

            generateTypeClass();
        }
    }

    public class LUISEntityGen7 : LUISEntityGen
    {
        public LUISEntityGen7(Indent indent, StreamWriter writer) : base(indent, writer)
        {
        }

        public override void generateBelongings(dynamic entity, List<string> names)
        {
            if (entity == null)
            {
                return;
            }

            foreach (dynamic belonging in entity)
            {
                string belongingString = (string)belonging.name;
                if (!entities.ContainsKey(belongingString))
                {
                    names.Add(belongingString);
                    entities[belongingString] = belonging;
                }
            }
        }

        public override void generateRoleBelongings(dynamic entity, List<string> names)
        {
            if (entity == null)
            {
                return;
            }

            foreach (dynamic belonging in entity)
            {
                string belongingString = (string)belonging;
                if (!entities.ContainsKey(belongingString))
                {
                    names.Add(belongingString);
                    entities[belongingString] = belonging;
                }
            }
        }


    }

    /*
     * Sentiment Type
     */
    public class LUISSentimentGen : LUISObjectGen
    {
        public LUISSentimentGen(Indent indent, StreamWriter writer) : base()
        {
            base.indent = indent;
            base.writer = writer;
        }

        public void generate()
        {
            WriteLine(++indent + "public class Sentiment");
            WriteLine(indent + "{");
            WriteLine(++indent + "public enum Label");
            WriteLine(indent + "{");
            WriteLine(++indent + "Positive,");
            WriteLine(indent + "Negative,");
            WriteLine(indent + "Neutral,");
            WriteLine(--indent + "}");
            WriteEmptyLine();

            WriteLine(indent + "public Label label { get; set; }");
            WriteLine(indent + "public float score { get; set; }");

            WriteLine(--indent + "}");
            --indent;
            WriteEmptyLine();
        }

    }
}
