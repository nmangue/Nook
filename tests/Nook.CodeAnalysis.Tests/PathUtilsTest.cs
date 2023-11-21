namespace Nook.CodeAnalysis.Tests;

public class PathUtilsTests
{
    [Theory]
    [InlineData("C:\\projects\\MyProj\\Components\\Greetings\\Hello.razor", "C:\\projects\\MyProj", "Components\\Greetings\\Hello.razor")]
    [InlineData("C:\\projects\\MyProj\\Components\\Greetings\\Subfolder\\Hi.razor", "C:\\projects\\MyProj", "Components\\Greetings\\Subfolder\\Hi.razor")]
    [InlineData("/home/user/projects/MyProj/Components/Greetings/Hello.razor", "/home/user/projects/MyProj", "Components\\Greetings\\Hello.razor")]
    [InlineData("/home/user/projects/MyProj/Components/Greetings/Subfolder/Hi.razor", "/home/user/projects/MyProj", "Components\\Greetings\\Subfolder\\Hi.razor")]
    public void GetRelativePath_Test(string path, string relativeTo, string expected)
    {
        string result = PathUtils.GetRelativePath(relativeTo, path);
        Assert.Equal(expected, result);

        // Check that we get the equivalent path when the result is combined with the sources
        Assert.Equal(
            Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar),
            Path.GetFullPath(Path.Combine(Path.GetFullPath(relativeTo), result))
                .TrimEnd(Path.DirectorySeparatorChar),
            ignoreCase: true,
            // ReSharper disable RedundantArgumentDefaultValue
            ignoreLineEndingDifferences: false,
            ignoreWhiteSpaceDifferences: false);
    }
}
