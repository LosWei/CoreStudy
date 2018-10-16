using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Aop.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceCollection services=new  ServiceCollection();
            //注册服务
            services.AddDynamicProxy();

            services.AddTransient<ISql, MySql>();
            //替换 默认的
            var provider = services.BuildAspectCoreServiceProvider();
            var mysql = provider.GetService<ISql>();
            System.Console.WriteLine("第一次获取");
            System.Console.WriteLine(mysql.GetData(1));
            System.Console.WriteLine("第二次获取");
            System.Console.WriteLine(mysql.GetData(1));
            System.Console.ReadKey();
        }
    }
    /// <summary>
    /// 缓存AOP
    /// </summary>
    public class MyCacheInterceptorAttribute : AbstractInterceptorAttribute
    {
        public  Dictionary<string,string> cacheDic=new Dictionary<string, string>();
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var cacheKey = string.Join("_", context.Parameters);
            if (cacheDic.ContainsKey(cacheKey))
            {
                context.ReturnValue = cacheDic[cacheKey];
                //拿到缓存 中止方法的执行
                return  Task.CompletedTask;
            }
            //执行代码
            var task = next(context);
            //取得结果
            var val = context.ReturnValue;
            //缓存结果
            cacheDic[cacheKey] = $"From Cache:{val}";
            return task;
        }
    }

    /// <summary>
    /// 日志 AOP
    /// </summary>
    public class MyLogInterceptorAttribute : AbstractInterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            System.Console.WriteLine("开始记录日志");
            var ps = context.Parameters;
            foreach (var o in ps)
            {
                System.Console.WriteLine($"ps:{o.GetType()}:{o.ToString()}"); 
            }
            //执行代码
            var task = next(context);
            System.Console.WriteLine("结束记录日志");
            return task;
        }
    }

    public interface ISql
    {
        [MyLogInterceptorAttribute, MyCacheInterceptorAttribute]
        string GetData(long id);
    }

    public class MySql: ISql
    {
        
        public string GetData(long  id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            return $"{id}的数据";
        }

       
    }
}
