namespace tWpfMashUp_v0._0._1.Extentions
{
    public static class ValidationExtention
    {
        /// <param name="str">string to check</param>
        /// <returns>true if the provided string is Empty, Null or contains white space</returns>
        public static bool IsEmptyNullOrWhiteSpace(this string str) => string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
    }
}
