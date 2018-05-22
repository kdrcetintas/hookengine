# hookengine
Information:

It's dynamic hook generator and caller and usable with all .NET projects.
Also has an callback at project startup (for web projects at IIS run) or (for desktop projects at first run)

Example Usage:

	#WebApiConfig.cs
	kdrcts.kHelpers.HookEngine.Start(
		new kdrcts.kHelpers.HookEngine.HookOptions()
		{
			ThrowErrors = false
		},
		System.Reflection.Assembly.GetExecutingAssembly()
	);
	
 
	
	#SampleController1.cs
	[kdrcts.kHelpers.HookEngine.HookEngineAppStartupMethod]
	public static void _sampleController1RunCallback()
	{
		kdrcts.kHelpers.HookEngine.addHookType("function1Runned"); // Generate hook list.
	}
	
	[HttpGet]
	public object sampleFunction1(int PK) {
		Console.WriteLine("function run stuffs");
		Console.WriteLine("function run stuffs");
		kdrcts.kHelpers.HookEngine.callHooks("function1Runned", new object[] { PK });
	}
	
 
	
	#SampleController2.cs
	[kdrcts.kHelpers.HookEngine.HookEngineAppStartupMethod]
	public static void _sampleController2RunCallback()
	{
		kdrcts.kHelpers.HookEngine.addHook("function1Runned", new kdrcts.kHelpers.HookEngine.Hook()
		{
			Priority = 10,
			Target = sampleFunction1Callback
		}); // register to a hook list
	}
	
	private void sampleFunction1Callback(object[] _params) {
		int PK = 0;
		int.TryParse(_params[0].ToString(), out PK);
		if (PK > 0) {
			Console.WriteLine("Callback comed with {0}", PK.ToString());
		}
	}
