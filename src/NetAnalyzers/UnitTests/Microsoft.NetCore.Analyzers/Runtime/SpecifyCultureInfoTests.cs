// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.SpecifyCultureInfoAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpSpecifyCultureInfoFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.SpecifyCultureInfoAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicSpecifyCultureInfoFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class SpecifyCultureInfoTests
    {
        [Fact]
        public async Task CA1304_PlainString_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Globalization;

public class CultureInfoTestClass0
{
    public string SpecifyCultureInfo01()
    {
        return ""foo"".ToLower();
    }
}",
            GetCSharpResultAt(9, 16, SpecifyCultureInfoAnalyzer.Rule, "string.ToLower()", "CultureInfoTestClass0.SpecifyCultureInfo01()", "string.ToLower(CultureInfo)"));
        }

        [Fact]
        public async Task CA1304_VariableStringInsideDifferentContainingSymbols_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Globalization;

public class CultureInfoTestClass1
{
    public string LowercaseAString(string name)
    {
        return name.ToLower();
    }

    public string InsideALambda(string insideLambda)
    {
        Func<string> ddd = () =>
        {
            return insideLambda.ToLower();
        };

        return null;
    }

    public string PropertyWithALambda
    {
        get
        {
            Func<string> ddd = () =>
            {
                return ""InsideGetter"".ToLower();
            };

            return null;
        }
    }
}
",
            GetCSharpResultAt(9, 16, SpecifyCultureInfoAnalyzer.Rule, "string.ToLower()", "CultureInfoTestClass1.LowercaseAString(string)", "string.ToLower(CultureInfo)"),
            GetCSharpResultAt(16, 20, SpecifyCultureInfoAnalyzer.Rule, "string.ToLower()", "CultureInfoTestClass1.InsideALambda(string)", "string.ToLower(CultureInfo)"),
            GetCSharpResultAt(28, 24, SpecifyCultureInfoAnalyzer.Rule, "string.ToLower()", "CultureInfoTestClass1.PropertyWithALambda.get", "string.ToLower(CultureInfo)"));
        }

        [Fact]
        public async Task CA1304_MethodOverloadHasCultureInfoAsFirstArgument_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Globalization;

public class CultureInfoTestClass2
{
    public static void Method()
    {
        MethodOverloadHasCultureInfoAsFirstArgument(""Foo"");
    }

    public static void MethodOverloadHasCultureInfoAsFirstArgument(string format)
    {
        MethodOverloadHasCultureInfoAsFirstArgument(CultureInfo.CurrentCulture, format);
    }

    public static void MethodOverloadHasCultureInfoAsFirstArgument(CultureInfo provider, string format)
    {
        Console.WriteLine(string.Format(provider, format));
    }
}",
            GetCSharpResultAt(9, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsFirstArgument(string)", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsFirstArgument(CultureInfo, string)"));
        }

        [Fact]
        public async Task CA1304_MethodOverloadHasCultureInfoAsFirstAndSecondArgument_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Globalization;

public class CultureInfoTestClass2
{
    public static void Method(CultureInfo provider)
    {
        MethodOverloadHasCultureInfoAsFirstAndSecondArgument(ref provider);
    }

    public static void MethodOverloadHasCultureInfoAsFirstAndSecondArgument(ref CultureInfo provider)
    {
        MethodOverloadHasCultureInfoAsFirstAndSecondArgument(ref provider, CultureInfo.CurrentCulture);
    }

    public static void MethodOverloadHasCultureInfoAsFirstAndSecondArgument(ref CultureInfo provider, CultureInfo provider2)
    {
    }
}",
            // Test0.cs(9,9): warning CA1304: The behavior of 'CultureInfoTestClass2.MethodOverloadHasCultureInfoAsFirstAndSecondArgument(ref CultureInfo)' could vary based on the current user's locale settings. Replace this call in 'CultureInfoTestClass2.Method(CultureInfo)' with a call to 'CultureInfoTestClass2.MethodOverloadHasCultureInfoAsFirstAndSecondArgument(ref CultureInfo, CultureInfo)'.
            GetCSharpResultAt(9, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsFirstAndSecondArgument(ref CultureInfo)", "CultureInfoTestClass2.Method(CultureInfo)", "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsFirstAndSecondArgument(ref CultureInfo, CultureInfo)"));
        }

        [Fact]
        public async Task CA1304_MethodOverloadHasCultureInfoAsFirstArgument_RefKindRef_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Globalization;

public class CultureInfoTestClass2
{
    public static void Method()
    {
        MethodOverloadHasCultureInfoAsFirstArgument(""Foo"");
    }

    public static void MethodOverloadHasCultureInfoAsFirstArgument(string format)
    {
        var provider = CultureInfo.CurrentCulture;
        MethodOverloadHasCultureInfoAsFirstArgument(ref provider, format);
    }

    public static void MethodOverloadHasCultureInfoAsFirstArgument(ref CultureInfo provider, string format)
    {
        Console.WriteLine(string.Format(provider, format));
    }
}");
        }

        [Fact]
        public async Task CA1304_MethodOverloadHasCultureInfoAsLastArgument_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Globalization;

public class CultureInfoTestClass2
{
    public static void Method()
    {
        MethodOverloadHasCultureInfoAsLastArgument(""Foo"");
    }

    public static void MethodOverloadHasCultureInfoAsLastArgument(string format)
    {
        MethodOverloadHasCultureInfoAsLastArgument(format, CultureInfo.CurrentCulture);
    }

    public static void MethodOverloadHasCultureInfoAsLastArgument(string format, CultureInfo provider)
    {
        Console.WriteLine(string.Format(provider, format));
    }

    public static void MethodOverloadHasCultureInfoAsLastArgument(CultureInfo provider, string format)
    {
        Console.WriteLine(string.Format(provider, format));
    }
}",
            GetCSharpResultAt(9, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsLastArgument(string)", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsLastArgument(string, CultureInfo)"));
        }

        [Fact]
        public async Task CA1304_MethodOverloadHasCultureInfoAsLastArgument_RefKindOut_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Globalization;

public class CultureInfoTestClass2
{
    public static void Method()
    {
        MethodOverloadHasCultureInfoAsLastArgument(""Foo"");
    }

    public static void MethodOverloadHasCultureInfoAsLastArgument(string format)
    {
        CultureInfo provider;
        MethodOverloadHasCultureInfoAsLastArgument(format, out provider);
    }

    public static void MethodOverloadHasCultureInfoAsLastArgument(string format, out CultureInfo provider)
    {
        provider = CultureInfo.CurrentCulture;
        Console.WriteLine(string.Format(provider, format));
    }
}");
        }

        [Fact]
        public async Task CA1304_MethodOverloadHasJustCultureInfo_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Globalization;

public class CultureInfoTestClass2
{
    public static void Method()
    {
        MethodOverloadHasJustCultureInfo();
    }

    public static void MethodOverloadHasJustCultureInfo()
    {
        MethodOverloadHasJustCultureInfo(CultureInfo.CurrentCulture);
    }

    public static void MethodOverloadHasJustCultureInfo(CultureInfo provider)
    {
        Console.WriteLine(string.Format(provider, """"));
    }
}",
            GetCSharpResultAt(9, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadHasJustCultureInfo()", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadHasJustCultureInfo(CultureInfo)"));
        }

        [Fact]
        public async Task CA1304_DoesNotRecommendObsoleteOverload_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Globalization;

public class CultureInfoTestClass2
{
    public static void Method()
    {
        MethodOverloadHasJustCultureInfo();
    }

    public static void MethodOverloadHasJustCultureInfo()
    {
        MethodOverloadHasJustCultureInfo(CultureInfo.CurrentCulture);
    }

    [Obsolete]
    public static void MethodOverloadHasJustCultureInfo(CultureInfo provider)
    {
        Console.WriteLine(string.Format(provider, """"));
    }
}");
        }

        [Fact]
        public async Task CA1304_TargetMethodIsGenericsAndNonGenerics_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Globalization;

public class CultureInfoTestClass2
{
    public static void Method()
    {
        TargetMethodIsNonGenerics();
        TargetMethodIsGenerics<int>(); // No Diagnostics
    }

    public static void TargetMethodIsNonGenerics()
    {
    }

    public static void TargetMethodIsNonGenerics<T>(CultureInfo provider)
    {
    }

    public static void TargetMethodIsGenerics<V>()
    {
    }

    public static void TargetMethodIsGenerics(CultureInfo provider)
    {
    }
}",
            GetCSharpResultAt(9, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.TargetMethodIsNonGenerics()", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.TargetMethodIsNonGenerics<T>(CultureInfo)"));
        }

        [Fact]
        public async Task CA1304_MethodOverloadIncludeNonCandidates_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Globalization;

public class CultureInfoTestClass2
{
    public static void Method()
    {
        MethodOverloadCount3();
    }

    public static void MethodOverloadCount3()
    {
        MethodOverloadCount3(CultureInfo.CurrentCulture);
    }

    public static void MethodOverloadCount3(CultureInfo provider)
    {
        Console.WriteLine(string.Format(provider, """"));
    }
    public static void MethodOverloadCount3(string b)
    {
    }
}",
            GetCSharpResultAt(9, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadCount3()", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadCount3(CultureInfo)"));
        }

        [Fact]
        public async Task CA1304_MethodOverloadWithJustCultureInfoAsExtraParameter_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Globalization;

public class CultureInfoTestClass2
{
    public static void Method()
    {
        MethodOverloadWithJustCultureInfoAsExtraParameter(2, 3);
    }

    public static void MethodOverloadWithJustCultureInfoAsExtraParameter(int a, int b)
    {
        MethodOverloadWithJustCultureInfoAsExtraParameter(a, b, CultureInfo.CurrentCulture);
    }

    public static void MethodOverloadWithJustCultureInfoAsExtraParameter(int a, int b, CultureInfo provider)
    {
        Console.WriteLine(string.Format(provider, """"));
    }
}",
            GetCSharpResultAt(9, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadWithJustCultureInfoAsExtraParameter(int, int)", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadWithJustCultureInfoAsExtraParameter(int, int, CultureInfo)"));
        }

        [Fact]
        public async Task CA1304_NoDiagnostics_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Globalization;

public class CultureInfoTestClass2
{
    public static void Method()
    {
        // No Diag - Inherited CultureInfo
        MethodOverloadHasInheritedCultureInfo(""Foo"");
        // No Diag - Since the overload has more parameters apart from CultureInfo
        MethodOverloadHasMoreThanCultureInfo(""Foo"");
        // No Diag - Since the CultureInfo parameter is neither as the first parameter nor as the last parameter
        MethodOverloadWithJustCultureInfoAsInbetweenParameter("""", """");
        // No Diag - Since the non-CultureInfo parameter in the overload has RefKind != RefKind.None
        MethodOverloadHasNonCultureInfoWithRefKindRef("""");
    }

    public static void MethodOverloadHasInheritedCultureInfo(string format)
    {
        MethodOverloadHasInheritedCultureInfo(new DerivedCultureInfo(""""), format);
    }

    public static void MethodOverloadHasInheritedCultureInfo(DerivedCultureInfo provider, string format)
    {
        Console.WriteLine(string.Format(provider, format));
    }

    public static void MethodOverloadHasMoreThanCultureInfo(string format)
    {
        MethodOverloadHasMoreThanCultureInfo(format, null, CultureInfo.CurrentCulture);
    }

    public static void MethodOverloadHasMoreThanCultureInfo(string format, string what, CultureInfo provider)
    {
        Console.WriteLine(string.Format(provider, format));
    }

    public static void MethodOverloadWithJustCultureInfoAsInbetweenParameter(string a, string b)
    {
        MethodOverloadWithJustCultureInfoAsInbetweenParameter(a, CultureInfo.CurrentCulture, b);
    }

    public static void MethodOverloadWithJustCultureInfoAsInbetweenParameter(string a, CultureInfo provider, string b)
    {
        Console.WriteLine(string.Format(provider, """"));
    }

    public static void MethodOverloadHasNonCultureInfoWithRefKindRef(string format)
    {
        MethodOverloadHasNonCultureInfoWithRefKindRef(ref format, CultureInfo.CurrentCulture);
    }

    public static void MethodOverloadHasNonCultureInfoWithRefKindRef(ref string format, CultureInfo provider)
    {
        Console.WriteLine(string.Format(provider, format));
    }
}

public class DerivedCultureInfo : CultureInfo
{
    public DerivedCultureInfo(string name):
        base(name)
    {
    }
}");
        }

        [Fact]
        public async Task CA1304_PlainString_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Globalization

Public Class CultureInfoTestClass0
    Public Function SpecifyCultureInfo01() As String
        Return ""foo"".ToLower()
    End Function
End Class",
            GetBasicResultAt(7, 16, SpecifyCultureInfoAnalyzer.Rule, "String.ToLower()", "CultureInfoTestClass0.SpecifyCultureInfo01()", "String.ToLower(CultureInfo)"));
        }

        [Fact]
        public async Task CA1304_VariableStringInsideDifferentContainingSymbols_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Globalization

Public Class CultureInfoTestClass1
    Public Function LowercaseAString(name As String) As String
        Return name.ToLower()
    End Function

    Public Function InsideALambda(insideLambda As String) As String
        Dim ddd As Func(Of String) = Function()
                                        Return insideLambda.ToLower()
                                     End Function

        Return Nothing
    End Function

    Public ReadOnly Property PropertyWithALambda() As String
        Get
            Dim ddd As Func(Of String) = Function()
                                            Return ""InsideGetter"".ToLower()
                                         End Function
            Return Nothing
        End Get
    End Property
End Class",
            GetBasicResultAt(7, 16, SpecifyCultureInfoAnalyzer.Rule, "String.ToLower()", "CultureInfoTestClass1.LowercaseAString(String)", "String.ToLower(CultureInfo)"),
            GetBasicResultAt(12, 48, SpecifyCultureInfoAnalyzer.Rule, "String.ToLower()", "CultureInfoTestClass1.InsideALambda(String)", "String.ToLower(CultureInfo)"),
            GetBasicResultAt(21, 52, SpecifyCultureInfoAnalyzer.Rule, "String.ToLower()", "CultureInfoTestClass1.PropertyWithALambda()", "String.ToLower(CultureInfo)"));
        }

        [Fact]
        public async Task CA1304_MethodOverloadHasCultureInfoAsFirstArgument_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Globalization

Public Class CultureInfoTestClass2
    Public Shared Sub Method()
        MethodOverloadHasCultureInfoAsFirstArgument(""Foo"")
    End Sub

    Public Shared Sub MethodOverloadHasCultureInfoAsFirstArgument(format As String)
        MethodOverloadHasCultureInfoAsFirstArgument(CultureInfo.CurrentCulture, format)
    End Sub

    Public Shared Sub MethodOverloadHasCultureInfoAsFirstArgument(provider As CultureInfo, format As String)
        Console.WriteLine(String.Format(provider, format))
    End Sub
End Class",
            GetBasicResultAt(7, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsFirstArgument(String)", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsFirstArgument(CultureInfo, String)"));
        }

        [Fact]
        public async Task CA1304_MethodOverloadHasCultureInfoAsLastArgument_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Globalization

Public Class CultureInfoTestClass2
    Public Shared Sub Method()
        MethodOverloadHasCultureInfoAsLastArgument(""Foo"")
    End Sub

    Public Shared Sub MethodOverloadHasCultureInfoAsLastArgument(format As String)
        MethodOverloadHasCultureInfoAsLastArgument(format, CultureInfo.CurrentCulture)
    End Sub

    Public Shared Sub MethodOverloadHasCultureInfoAsLastArgument(format As String, provider As CultureInfo)
        Console.WriteLine(String.Format(provider, format))
    End Sub

    Public Shared Sub MethodOverloadHasCultureInfoAsLastArgument(provider As CultureInfo, format As String)
        Console.WriteLine(String.Format(provider, format))
    End Sub
End Class",
            GetBasicResultAt(7, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsLastArgument(String)", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadHasCultureInfoAsLastArgument(String, CultureInfo)"));
        }

        [Fact]
        public async Task CA1304_MethodOverloadHasJustCultureInfo_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Globalization

Public Class CultureInfoTestClass2
    Public Shared Sub Method()
        MethodOverloadHasJustCultureInfo()
    End Sub

    Public Shared Sub MethodOverloadHasJustCultureInfo()
        MethodOverloadHasJustCultureInfo(CultureInfo.CurrentCulture)
    End Sub

    Public Shared Sub MethodOverloadHasJustCultureInfo(provider As CultureInfo)
        Console.WriteLine(String.Format(provider, """"))
    End Sub
End Class",
            GetBasicResultAt(7, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadHasJustCultureInfo()", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadHasJustCultureInfo(CultureInfo)"));
        }

        [Fact]
        public async Task CA1304_MethodOverloadIncludeNonCandidates_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Globalization

Public Class CultureInfoTestClass2
    Public Shared Sub Method()
        MethodOverloadCount3()
    End Sub

    Public Shared Sub MethodOverloadCount3()
        MethodOverloadCount3(CultureInfo.CurrentCulture)
    End Sub

    Public Shared Sub MethodOverloadCount3(provider As CultureInfo)
        Console.WriteLine(String.Format(provider, """"))
    End Sub

    Public Shared Sub MethodOverloadCount3(b As String)
    End Sub
End Class",
            GetBasicResultAt(7, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadCount3()", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadCount3(CultureInfo)"));
        }

        [Fact]
        public async Task CA1304_MethodOverloadWithJustCultureInfoAsExtraParameter_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Globalization

Public Class CultureInfoTestClass2
    Public Shared Sub Method()
        MethodOverloadWithJustCultureInfoAsExtraParameter(2, 3)
    End Sub

    Public Shared Sub MethodOverloadWithJustCultureInfoAsExtraParameter(a As Integer, b As Integer)
        MethodOverloadWithJustCultureInfoAsExtraParameter(a, b, CultureInfo.CurrentCulture)
    End Sub

    Public Shared Sub MethodOverloadWithJustCultureInfoAsExtraParameter(a As Integer, b As Integer, provider As CultureInfo)
        Console.WriteLine(String.Format(provider, """"))
    End Sub
End Class",
            GetBasicResultAt(7, 9, SpecifyCultureInfoAnalyzer.Rule, "CultureInfoTestClass2.MethodOverloadWithJustCultureInfoAsExtraParameter(Integer, Integer)", "CultureInfoTestClass2.Method()", "CultureInfoTestClass2.MethodOverloadWithJustCultureInfoAsExtraParameter(Integer, Integer, CultureInfo)"));
        }

        [Fact]
        public async Task CA1304_NoDiagnostics_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Globalization

Public Class CultureInfoTestClass2
    Public Shared Sub Method()
        ' No Diag - Inherited CultureInfo
        MethodOverloadHasInheritedCultureInfo(""Foo"")
        ' No Diag - There are more parameters apart from CultureInfo
        MethodOverloadHasMoreThanCultureInfo(""Foo"")
        ' No Diag - The CultureInfo parameter is neither the first parameter nor the last parameter
        MethodOverloadWithJustCultureInfoAsInbetweenParameter("""", """")
    End Sub

    Public Shared Sub MethodOverloadHasInheritedCultureInfo(format As String)
        MethodOverloadHasInheritedCultureInfo(New DerivedCultureInfo(""""), format)
    End Sub

    Public Shared Sub MethodOverloadHasInheritedCultureInfo(provider As DerivedCultureInfo, format As String)
        Console.WriteLine(String.Format(provider, format))
    End Sub

    Public Shared Sub MethodOverloadHasMoreThanCultureInfo(format As String)
        MethodOverloadHasMoreThanCultureInfo(format, Nothing, CultureInfo.CurrentCulture)
    End Sub

    Public Shared Sub MethodOverloadHasMoreThanCultureInfo(format As String, what As String, provider As CultureInfo)
        Console.WriteLine(String.Format(provider, format))
    End Sub

    Public Shared Sub MethodOverloadWithJustCultureInfoAsInbetweenParameter(a As String, b As String)
        MethodOverloadWithJustCultureInfoAsInbetweenParameter(a, CultureInfo.CurrentCulture, b)
    End Sub

    Public Shared Sub MethodOverloadWithJustCultureInfoAsInbetweenParameter(a As String, provider As CultureInfo, b As String)
        Console.WriteLine(String.Format(provider, """"))
    End Sub
End Class

Public Class DerivedCultureInfo
    Inherits CultureInfo
    Public Sub New(name As String)
        MyBase.New(name)
    End Sub
End Class");
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column, DiagnosticDescriptor rule, string arg1, string arg2, string arg3) =>
            new DiagnosticResult(rule)
                .WithLocation(line, column)
                .WithArguments(arg1, arg2, arg3);

        private DiagnosticResult GetBasicResultAt(int line, int column, DiagnosticDescriptor rule, string arg1, string arg2, string arg3) =>
            new DiagnosticResult(rule)
                .WithLocation(line, column)
                .WithArguments(arg1, arg2, arg3);
    }
}