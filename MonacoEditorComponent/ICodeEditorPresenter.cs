using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Uno.Foundation;
using Uno.Foundation.Interop;

namespace Monaco
{
	public interface ICodeEditorPresenter
	{
		/// <summary>Adds a native Windows Runtime object as a global parameter to the top level document inside of a WebView.</summary>
		/// <param name="name">The name of the object to expose to the document in the WebView.</param>
		/// <param name="pObject">The object to expose to the document in the WebView.</param>
		void AddWebAllowedObject(string name, object pObject);

		// <summary>Occurs when a user performs an action in a WebView that causes content to be opened in a new window.</summary>
		event TypedEventHandler<WebView, WebViewNewWindowRequestedEventArgs> NewWindowRequested;

		/// <summary>Occurs before the WebView navigates to new content.</summary>
		event TypedEventHandler<WebView, WebViewNavigationStartingEventArgs> NavigationStarting;

		/// <summary>Occurs when the WebView has finished parsing the current HTML content.</summary>
		event TypedEventHandler<WebView, WebViewDOMContentLoadedEventArgs> DOMContentLoaded;

		/// <summary>Occurs when the WebView has finished loading the current content or if navigation has failed.</summary>
		event TypedEventHandler<WebView, WebViewNavigationCompletedEventArgs> NavigationCompleted;

		/// <summary>Gets or sets the Uniform Resource Identifier (URI) source of the HTML content to display in the WebView control.</summary>
		/// <returns>The Uniform Resource Identifier (URI) source of the HTML content to display in the WebView control.</returns>
		global::System.Uri Source { get; set; }

		CoreDispatcher Dispatcher { get; }

		IAsyncOperation<string> InvokeScriptAsync(string scriptName, IEnumerable<string> arguments);

		bool Focus(FocusState state);
	}

	public partial class CodeEditorPresenter : WebView, ICodeEditorPresenter
	{
	}

	public partial class WasmCodeEditor : Control, ICodeEditorPresenter, IJSObject
	{
		private readonly JSObjectHandle _handle;

		/// <inheritdoc />
		JSObjectHandle IJSObject.Handle => _handle;

		public WasmCodeEditor() : base("iframe")
		{
			//Background = new SolidColorBrush(Colors.Red);
			RaiseDOMContentLoaded();

			_handle = JSObjectHandle.Create(this);
			WebAssemblyRuntime.InvokeJSWithInterop($@"
				console.log(""///////////////////////////////// subscribing to DOMContentLoaded"");

				var frame = Uno.UI.WindowManager.current.getView({HtmlId});

				frame.addEventListener(""loadstart"", function(event) {{
					var frameDoc = frame.contentDocument;
					console.log(""/////////////////////////////////  Frame DOMContentLoaded, subscribing to document"" + frameDoc);
					{this}.RaiseDOMContentLoaded();
				}}); 



				frame.addEventListener(""load"", function(event) {{
					var frameDoc = frame.contentDocument;
					console.log(""/////////////////////////////////  Frame loaded, subscribing to document"" + frameDoc);
					{this}.RaiseDOMContentLoaded();
					//frameDoc.addEventListener(""DOMContentLoaded"", function(event) {{
					//	console.log(""Raising RaiseDOMContentLoaded"");
					//	{this}.RaiseDOMContentLoaded();
					//}});
				}}); ");
		}

		public void RaiseDOMContentLoaded()
		{
			if (_handle == null) return;

			Console.Error.WriteLine("-------------------------------------------------------- RaiseDOMContentLoaded");
			DOMContentLoaded?.Invoke(null, new WebViewDOMContentLoadedEventArgs());
		}

		/// <inheritdoc />
		protected override void OnLoaded()
		{
			base.OnLoaded();

			/*Console.Error.WriteLine("---------------------- LOADED ");


			var script = $@"
					var frame = Uno.UI.WindowManager.current.getView({HtmlId});
					var frameDoc = frame.contentDocument;
					
					return frameDoc.onload = function() { };

			Console.Error.WriteLine("***************************************** AddWebAllowedObject: " + script);*/
		}

		/// <inheritdoc />
		public void AddWebAllowedObject(string name, object pObject)
		{
			if (pObject is IJSObject obj)
			{
				var script = $@"
					var value = {obj.Handle.GetNativeInstance()};
					var frame = Uno.UI.WindowManager.current.getView({HtmlId});
					var frameWindow = frame.contentWindow;
					
					console.log(value);

					frameWindow.{name} = value;
					";
				////frameWindow.eval(""var {name} = window.parent.{obj.Handle.GetNativeInstance().Replace("\"", "\\\"")}; ""); 

				Console.Error.WriteLine("***************************************** AddWebAllowedObject: " + script);

				try
				{
					WebAssemblyRuntime.InvokeJS(script);
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("FAILED " + e);
				}
			}
			else
			{
				Console.Error.WriteLine(name + " is not a JSObject :( :( :( :( :( :( :( :( :( :( :( :( :( :( :( :( :( :( :( :( ");
			}
		}

		/// <inheritdoc />
		public event TypedEventHandler<WebView, WebViewNewWindowRequestedEventArgs> NewWindowRequested; // ignored for now (external navigation)

		/// <inheritdoc />
		public event TypedEventHandler<WebView, WebViewNavigationStartingEventArgs> NavigationStarting;

		/// <inheritdoc />
		public event TypedEventHandler<WebView, WebViewDOMContentLoadedEventArgs> DOMContentLoaded;

		/// <inheritdoc />
		public event TypedEventHandler<WebView, WebViewNavigationCompletedEventArgs> NavigationCompleted; // ignored for now (only focus the editor)

		/// <inheritdoc />
		public Uri Source
		{
			get => new Uri(GetAttribute("src"));
			set
			{
				var target = value.IsFile 
					? value.PathAndQuery 
					: value.ToString();

				Console.Error.WriteLine("***** LOADING: " + target);

				SetAttribute("src", target);

				Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => NavigationStarting?.Invoke(null, new WebViewNavigationStartingEventArgs()));
			}
		}

		/// <inheritdoc />
		public IAsyncOperation<string> InvokeScriptAsync(string scriptName, IEnumerable<string> arguments)
		{
			var script = $@"(function() {{
				var frame = Uno.UI.WindowManager.current.getView({HtmlId});
				var frameWindow = frame.contentWindow;
				
				try {{
					frameWindow.__evalMethod = function() {{ {arguments.Single()} }};
					
					return frameWindow.eval(""__evalMethod()"") || """";
				}}
				finally {{
					frameWindow.__evalMethod = null;
				}}
			}})()";
			Console.Error.WriteLine(script);

			try
			{
				var result = WebAssemblyRuntime.InvokeJS(script);

				Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
				Console.WriteLine(result);

				return Task.FromResult(result).AsAsyncOperation();
			}
			catch (Exception e)
			{
				Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
				Console.WriteLine(e);

				return Task.FromResult("").AsAsyncOperation();
			}
		}
	}
}