# 基于Roslyn实现的一个动态编译器

## 用法
### 基础用法
```cs
 DynamicCoding dynamicCoding = new DynamicCoding();
 Assembly xxxAssembly = typeof(xxx).Assembly;
 dynamicCoding.References.Add(xxxAssembly.Location);
 dynamicCoding.Using.Add("Newtonsoft.Json");
 dynamicCoding.Code = "var a = 1;var b = 2;var c=a+b;return c;"
 dynamicCoding.ReturnType = typeof(int);
 var result = (int)dynamicCoding.Invoke();
```
- 可以预见，最后result==3

### 进阶用法，使用自定义的方法
- 假设定义了一个类，类的定义如下
```cs
namespace TestNamespace{
  public static class TestExt{
    public static string Int2String(this int val)
    {
      return val.ToString();
    }
   }
}
```
- 那么可以这样用
```cs
 DynamicCoding dynamicCoding = new DynamicCoding();
 Assembly testAssembly = typeof(TestExt).Assembly;
 dynamicCoding.References.Add(testAssembly.Location);
 dynamicCoding.Using.Add("TestNamespace");
 dynamicCoding.Code = "var a = 1;var b = 2;var c=a+b;return c.Int2String();"
 dynamicCoding.ReturnType = typeof(string);
 var result = dynamicCoding.Invoke().ToString();
```
- 可以预见，最后result=="3"
