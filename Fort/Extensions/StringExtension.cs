namespace Fort.Extensions
{
  public static class StringExtension
  {
    public static string ToCamelCase(this string self)
    {
      if (self.Length <= 1)
        return self.ToUpper();

      return $"{self.Substring(0, 1).ToUpper()}{self.Substring(1)}";
    }
  }
}