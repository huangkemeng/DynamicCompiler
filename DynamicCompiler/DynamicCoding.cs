using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KemengSoft.DynamicCompiler
{
    public class DynamicCoding
    {
        /// <summary>
        /// 源码片段
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 基础命名空间
        /// </summary>
        private readonly static List<string> baseUsing = new List<string>
        {
          "System",
          "System.IO",
          "System.Linq",
          "System.Text",
          "System.Text.RegularExpressions",
          "System.Collections.Generic"
        };
        /// <summary>
        /// 基础引用
        /// </summary>
        private readonly static List<string> baseReferences = new List<string>
        {
            Assembly.GetEntryAssembly().Location,
            Assembly.Load("System").Location,
            Assembly.Load("System.Runtime").Location,
            typeof (System.Object).Assembly.Location,
            typeof (System.Text.RegularExpressions.Regex).Assembly.Location,
            typeof (System.Linq.Enumerable).Assembly.Location,
            typeof (System.Collections.IList).Assembly.Location
        };

        /// <summary>
        /// 返回类型
        /// </summary>
        public Type ReturnType { get; set; } = typeof(void);
        /// <summary>
        /// 参数
        /// </summary>
        public List<DynamicCodingParams> InputParams { get; set; } = new List<DynamicCodingParams>();
        /// <summary>
        /// 引入命名空间
        /// </summary>
        public List<string> Using { get; set; } = baseUsing;
        /// <summary>
        /// 引入程序集
        /// </summary>
        public List<string> References { get; set; } = baseReferences;
        /// <summary>
        /// 构造
        /// </summary>
        public DynamicCoding()
        {

        }
        /// <summary>
        /// 构造
        /// </summary>
        public DynamicCoding(string code)
        {
            Code = code;
        }
        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        public object Invoke()
        {
            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(GetSourceCode());
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            CSharpCompilationOptions compilationOptions =
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithUsings(Using.Distinct());
            var compilation = CSharpCompilation
             .Create("WriteArticleByCode.dll")
             .AddSyntaxTrees(tree)
             .AddReferences(References.Distinct().Select(e => MetadataReference.CreateFromFile(e)))
             .WithOptions(compilationOptions);
            Assembly compiledAssembly = null;
            using (var stream = new MemoryStream())
            {
                var compileResult = compilation.Emit(stream);
                if (compileResult.Success)
                {
                    compiledAssembly = Assembly.Load(stream.GetBuffer());
                }
                else
                {
                    throw new InvalidOperationException($"{string.Join("\r\n", compileResult.Diagnostics.Select(x => x.GetMessage()))}");
                }
            }
            var calculatorClass = compiledAssembly.GetType("Program");
            var evaluateMethod = calculatorClass.GetMethod("Invoke");
            return evaluateMethod.Invoke(null, InputParams.Select(e => e.Value).ToArray());
        }
        /// <summary>
        /// 源码
        /// </summary>
        public string GetSourceCode()
        {
            string code = $@"
                   {string.Join("\r\n", Using.Distinct().Select(e => $"using {e};"))}
                   public class Program{{
                      public static {ReturnType.FullName} Invoke({string.Join(',', InputParams.Select(e => $"{e.Type.FullName} {e.Name}"))})
                      {{
                           {Code}
                      }}
                }}";
            return code;
        }
        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            Code = string.Empty;
            ReturnType = typeof(void);
            InputParams = new List<DynamicCodingParams>();
        }
        /// <summary>
        /// 强力清理
        /// </summary>
        public void PowerfulClear()
        {
            Clear();
            Using = baseUsing;
            References = baseReferences;
        }
    }
}
