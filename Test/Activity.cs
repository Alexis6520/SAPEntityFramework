namespace Test
{
    internal class Activity
    {
        public Activity() { }
        public Activity(string docNum) { DocNum = docNum; }
        public int ActivityCode { get; set; }
        public string DocNum { get; set; }
    }
}
