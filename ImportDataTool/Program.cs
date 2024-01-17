using MySqlConnector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using System.Text;

namespace ImportDataTool
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string jsonFilePath = "Setting.json";
            string filePath = "";
            Setting? setting = new Setting();
            MySqlConfig? mySqlConfig = new MySqlConfig();
            string jsonContent = File.ReadAllText(jsonFilePath);
            setting = JsonConvert.DeserializeObject<Setting>(jsonContent);

            if(setting != null ) 
            {
                mySqlConfig = setting.MySqlConfig;
                if (String.IsNullOrEmpty(setting.Date))
                {
                    DateTime date = DateTime.Now.AddDays(-1);
                    filePath = date.ToString("yyyyMMdd");
                }
                else
                {
                    filePath = setting.Date;
                }
            }
            else
            {
                throw new Exception("設定檔讀取不到");
            }

            

            try
            {
                string connectionString = "";
                if (mySqlConfig != null)
                {
                    connectionString = $"Server={mySqlConfig.Server};Port={mySqlConfig.Port};User ID={mySqlConfig.User};Password={mySqlConfig.Password};Database={mySqlConfig.Database};AllowLoadLocalInfile=true";
                }
                else
                {
                    throw new Exception($"{jsonFilePath} 設定錯誤");
                }
                if (File.Exists(filePath + ".csv"))
                {
                    Console.WriteLine("test:" + mySqlConfig.Columns);
                    await LoadCSVDataAsync(connectionString, filePath, mySqlConfig);
                }
                else
                {
                    throw new Exception($"{filePath}.csv 不存在");
                }
                
            }
            catch (Exception ex)
            {
                string csvFilePath = $"{filePath}_Error.log";
                StreamWriter sw = new StreamWriter(csvFilePath, false, Encoding.UTF8);
                sw.WriteLine(ex.Message);
                sw.Close();
            }
        }

        public static void JsonLoadSettings(string jsonFilePath, Setting? setting)
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            setting = JsonConvert.DeserializeObject<Setting>(jsonContent);
        }

        public static async Task LoadCSVDataAsync(string connectionString, string filePath, MySqlConfig? mySqlConfig)
        {
            DateTime startTime = DateTime.Now;
            if (mySqlConfig != null)
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    var bulkLoader = new MySqlBulkLoader(conn)
                    {
                        FileName = $"{filePath}.csv",
                        TableName = mySqlConfig.Table,
                        CharacterSet = "UTF8",
                        FieldTerminator = ",",
                        NumberOfLinesToSkip = 1,
                        LineTerminator = "\r\n",
                        FieldQuotationCharacter = '"',
                        FieldQuotationOptional = true,
                        Local = true,
                    };
                    bulkLoader.Columns.AddRange(mySqlConfig.Columns);
                    int rowsInserted = await bulkLoader.LoadAsync();
                    DateTime endTime = DateTime.Now;
                    StreamWriter sw = new StreamWriter($"{filePath}.log", false, Encoding.UTF8);
                    sw.WriteLine($"{filePath} 匯入成功，總共新增 {rowsInserted} 筆，執行時間:{(endTime - startTime).TotalSeconds}秒");
                    sw.Close();
                    
                }
            }
            
        }
    }
}