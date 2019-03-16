namespace Devil.GamePlay
{
    public class SitcomCompileException : System.Exception 
	{
        public SitcomCompileException(string msg) : base(msg) { }

        public SitcomCompileException(SitcomFile.Keyword keyword) : base(string.Format("Compile sitcom  error.\n\t {0}", keyword))
        { }
	}

    public class SitcomNullReferenceExpception: System.Exception
    {
        public SitcomNullReferenceExpception(string msg) : base(msg) { }

        public SitcomNullReferenceExpception(SitcomFile.Keyword keyword) : base(string.Format("Sitcom null reference exception:\n\t {0}", keyword))
        { }
    }
}