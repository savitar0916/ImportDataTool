# ImportDataTool

這是一個簡單的匯入小工具，可使用CSV來將資料大量匯入MySQL

## Setting.json

依據範例填入需要匯入的資料，此檔案需要產生才可以做匯入

### Version

版本可以隨意的填，無任何的用途純粹讓自己看

### Date

此為匯入檔案的檔名，通常檔名會取為YYYYMMDD.csv，而這邊只要附上YYYYMMDD，即可以讀取那個日期的檔名，若未填寫則是抓取當下時間的前一天


### MySqlConfig

此為要匯入的MySQL的機器資訊，記得要做匯入的時候需要確認MySQL機器裡面的local-infile需要設定成**ON**

### Columns

此為該Table的所有欄位，依據要匯入的檔案欄位以及Table的欄位去做輸入

```
{
  "Version": "24.01.15.1",
  "Date": "",
  "MySqlConfig": {
    "Id": 0,
    "Server": "192.168.13.140",
    "Port": 3306,
    "User": "aaa",
    "Password": "1234",
    "Database": "ContentBoard",
    "Table": "Content",
    "Columns": [
      "Id",
      "memberId",
      "content",
      "createAt",
      "updateAt",
      "deleteAt"
    ]
  }
}

```




