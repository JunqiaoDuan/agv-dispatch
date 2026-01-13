using DbUp;
using DbUp.Engine;
using Microsoft.Extensions.Configuration;

internal class Program
{
    static void Main(string[] args)
    {
        // 迁移数据
        StartDbMigrate();
    }

    // 启动数据迁移
    private static void StartDbMigrate()
    {
        // 从 appsettings.json 读取配置
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 获取连接字符串
        var connString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connString))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("错误：未在 appsettings.json 中找到连接字符串！");
            Console.WriteLine("请在 appsettings.json 中配置 ConnectionStrings:DefaultConnection");
            Console.ResetColor();
            Console.ReadKey();
            return;
        }

        // 初始化引擎
        var upgrader = DeployChanges.To
            .SqlDatabase(connString)
            .WithScriptsFromFileSystem(@".\Scripts", s => s.StartsWith(@".\Scripts\"))
            .LogToConsole()
            .Build();

        // 显示等待执行的脚本
        var waitingExecutedScripts = upgrader
            .GetScriptsToExecute();
        if (waitingExecutedScripts.Count == 0)
        {
            Console.WriteLine("========================没有需要执行的更新========================");
            return;
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("========================本次需要执行的脚本如下========================");
        foreach (var item in waitingExecutedScripts)
        {
            Console.WriteLine("--------脚本名称--------");
            Console.WriteLine(item.Name);
            Console.WriteLine("--------脚本内容--------");
            Console.WriteLine(item.Contents);
        }

        // 为了安全更新数据库,必须要手动确认
        Console.WriteLine("确认是否执行本次更新？（请确认已备份数据库）[y/n]");
        var inputKey = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(inputKey) || 
            (inputKey.ToLower() != "y" && inputKey.ToLower() != "yes"))
        {
            Console.WriteLine("========================您已拒绝本次更新========================");
            return;
        }

        // 执行迁移
        var result = upgrader.PerformUpgrade();

        // 失败
        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("========================更新失败========================");
            Console.WriteLine(result.Error);
            Console.ResetColor();
            Console.ReadKey();
        }
        else
        {
            // 成功
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("========================更新成功========================");
            Console.ResetColor();
        }
    }
}