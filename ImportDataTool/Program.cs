using MySqlConnector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;
using System.Net.Http.Json;
using System.Text;

namespace ImportDataTool
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string jsonFilePath = "Setting.json";
            string? filePathStart = "";
            string? filePathEnd = "";
            int successNum = 0;
            int failNum = 0;
            Setting? setting = new Setting();
            MySqlConfig? mySqlConfig = new MySqlConfig();
            string jsonContent = File.ReadAllText(jsonFilePath);
            setting = JsonConvert.DeserializeObject<Setting>(jsonContent);
            StringBuilder successFiles = new StringBuilder();
            StringBuilder failFiles = new StringBuilder();
            try
            {

                #region 讀取Setting.json

                Console.WriteLine("讀取Setting.json...");

                if (setting != null)
                {
                    if (String.IsNullOrEmpty(setting.DateStart) && String.IsNullOrEmpty(setting.DateEnd))
                    {
                        filePathStart = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                        filePathEnd = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                    }
                    else
                    {
                        filePathStart = setting.DateStart;
                        filePathEnd = setting.DateEnd;
                    }
                    mySqlConfig = setting.MySqlConfig;
                }
                else
                {
                    throw new Exception("設定檔讀取不到");
                }

                #endregion

                #region 設定MySQL參數

                Console.WriteLine("設定MySQL參數...");

                string connectionString = "";
                if (mySqlConfig != null)
                {
                    connectionString = $"Server={mySqlConfig.Server};Port={mySqlConfig.Port};User ID={mySqlConfig.User};Password={mySqlConfig.Password};Database={mySqlConfig.Database};AllowLoadLocalInfile=true";
                }
                else
                {
                    throw new Exception($"{jsonFilePath} 設定錯誤");
                }

                #endregion

                #region 日期驗證

                Console.WriteLine($"日期驗證中...");

                DateTime startDate;
                DateTime endDate;
                if (filePathEnd != null && filePathStart != null)
                {
                    startDate = DateTime.ParseExact(filePathStart, "yyyyMMdd", CultureInfo.InvariantCulture);
                    endDate = DateTime.ParseExact(filePathEnd, "yyyyMMdd", CultureInfo.InvariantCulture);
                    if (endDate < startDate)
                    {
                        throw new Exception($"結束日期不能比開始日期早，開始日期:{filePathStart}，結束日期:{filePathEnd}");
                    }
                }
                else
                {
                    throw new Exception($"日期設定錯誤，開始日期:{filePathStart}，結束日期:{filePathEnd}");
                }

                #endregion

                #region 資料匯入

                Console.WriteLine($"資料匯入中...");

                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    string dateString = date.ToString("yyyyMMdd");

                    if (File.Exists(dateString + ".csv"))
                    {
                        await LoadCSVDataAsync(connectionString, dateString, mySqlConfig, successFiles);
                        successNum++;

                        Console.WriteLine($"{dateString}.csv 匯入成功");

                    }
                    else
                    {
                        failNum++;
                        failFiles.AppendLine(dateString);

                        Console.WriteLine($"{dateString}.csv 不存在");
                    }
                    
                }

                #endregion

            }
            catch (Exception ex)
            {
                string csvFilePath = $"{filePathStart}to{filePathEnd}_Error.log";
                StreamWriter sw = new StreamWriter(csvFilePath, false, Encoding.UTF8);
                sw.WriteLine(ex.Message);
                sw.Close();
            }
            finally
            {

                Console.WriteLine($"{filePathStart}~{filePathEnd} 匯入成功");

                string csvFilePath = $"{filePathStart}_{filePathEnd}.log";
                StreamWriter sw = new StreamWriter(csvFilePath, false, Encoding.UTF8);
                sw.WriteLine($"開始日期:{filePathStart}");
                sw.WriteLine($"結束日期:{filePathEnd}");
                sw.WriteLine("------------------------------------------------------");
                sw.WriteLine($"成功檔案數:{successNum}個");
                sw.WriteLine($"成功的日期:");
                sw.WriteLine($"{successFiles}");
                sw.WriteLine("------------------------------------------------------");
                sw.WriteLine($"失敗檔案數:{failNum}個");
                sw.WriteLine($"失敗的日期:");
                sw.WriteLine($"{failFiles}");
                sw.Close();
            }
        }

        public static void JsonLoadSettings(string jsonFilePath, Setting? setting)
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            setting = JsonConvert.DeserializeObject<Setting>(jsonContent);
        }

        public static async Task LoadCSVDataAsync(string connectionString,
            string filePath,
            MySqlConfig? mySqlConfig,
            StringBuilder successFiles)
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
                    successFiles.AppendLine($"{filePath} 匯入成功，總共新增 {rowsInserted} 筆，執行時間:{(endTime - startTime).TotalSeconds}秒");
                }
            }

        }
    }
}