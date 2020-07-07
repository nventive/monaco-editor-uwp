﻿using Monaco.Languages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Foundation;

namespace Monaco
{
    /// <summary>
    /// Helper to static Monaco.Languages Namespace methods.
    /// https://microsoft.github.io/monaco-editor/api/modules/monaco.languages.html
    /// </summary>
    public sealed class LanguagesHelper
    {
        private readonly WeakReference<CodeEditor> _editor;

        [Obsolete("Use <Editor Instance>.Languages.* instead of constructing your own LanguagesHelper.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LanguagesHelper(CodeEditor editor) // TODO: Make Internal later.
        {
            // We need the editor component in order to execute JavaScript within 
            // the WebView environment to retrieve data (even though this Monaco class is static).
            _editor = new WeakReference<CodeEditor>(editor);
        }

        public IAsyncOperation<IList<ILanguageExtensionPoint>> GetLanguagesAsync()
        {
            if (_editor.TryGetTarget(out CodeEditor editor))
            {
                return editor.SendScriptAsync<IList<ILanguageExtensionPoint>>("monaco.languages.getLanguages()").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncAction RegisterAsync(ILanguageExtensionPoint language)
        {
            if (_editor.TryGetTarget(out CodeEditor editor))
            {
                return editor.InvokeScriptAsync("monaco.languages.register", language).AsAsyncAction();
            }

            return null;
        }

        public IAsyncAction RegisterCodeLensProviderAsync(string languageId, CodeLensProvider provider)
        {
            if (_editor.TryGetTarget(out CodeEditor editor))
            {
                editor._parentAccessor.RegisterEvent("ProvideCodeLenses" + languageId, async (args) =>
                {
                    var list = await provider.ProvideCodeLensesAsync(editor.GetModel());

                    if (list != null)
                    {
                        return JsonConvert.SerializeObject(list);
                    }

                    return null;
                });

                editor._parentAccessor.RegisterEvent("ResolveCodeLens" + languageId, async (args) =>
                {
                    if (args != null && args.Length >= 1)
                    {
                        var lens = await provider.ResolveCodeLensAsync(editor.GetModel(), JsonConvert.DeserializeObject<CodeLens>(args[0]));

                        if (lens != null)
                        {
                            return JsonConvert.SerializeObject(lens);
                        }
                    }

                    return null;
                });

                return editor.InvokeScriptAsync("registerCodeLensProvider", new object[] { languageId }).AsAsyncAction();
            }

            return null;
        }

        public IAsyncAction RegisterColorProviderAsync(string languageId, DocumentColorProvider provider)
        {
            if (_editor.TryGetTarget(out CodeEditor editor))
            {
                editor._parentAccessor.RegisterEvent("ProvideColorPresentations" + languageId, async (args) =>
                {
                    if (args != null && args.Length >= 1)
                    {
                        var items = await provider.ProvideColorPresentationsAsync(editor.GetModel(), JsonConvert.DeserializeObject<ColorInformation>(args[0]));

                        if (items != null)
                        {
                            return JsonConvert.SerializeObject(items);
                        }
                    }

                    return null;
                });

                editor._parentAccessor.RegisterEvent("ProvideDocumentColors" + languageId, async (args) =>
                {
                    var items = await provider.ProvideDocumentColorsAsync(editor.GetModel());

                    if (items != null)
                    {
                        return JsonConvert.SerializeObject(items);
                    }

                    return null;
                });

                return editor.InvokeScriptAsync("registerColorProvider", new object[] { languageId }).AsAsyncAction();
            }

            return null;
        }

        public IAsyncAction RegisterCompletionItemProviderAsync(string languageId, CompletionItemProvider provider)
        {
            if (_editor.TryGetTarget(out CodeEditor editor))
            {
                // Wrapper around CompletionItem Provider to Monaco editor.
                // TODO: Add Incremented Id so that we can register multiple providers per language?
                editor._parentAccessor.RegisterEvent("CompletionItemProvider" + languageId, async (args) =>
                {
                    if (args != null && args.Length >= 2)
                    {
                        var items = await provider.ProvideCompletionItemsAsync(editor.GetModel(), JsonConvert.DeserializeObject<Position>(args[0]), JsonConvert.DeserializeObject<CompletionContext>(args[1]));

                        if (items != null)
                        {
                            System.Diagnostics.Debug.WriteLine("Items: " + items);
                            var serialized= JsonConvert.SerializeObject(items);
                            System.Diagnostics.Debug.WriteLine("Items in JSON: " + serialized);
                            return serialized;
                        }
                    }

                    return null;
                });

                editor._parentAccessor.RegisterEvent("CompletionItemRequested" + languageId, async (args) =>
                {
                    if (args != null && args.Length >= 2)
                    {
                        var position = JsonConvert.DeserializeObject<Position>(args[0]);
                        var requestedItem = JsonConvert.DeserializeObject<CompletionItem>(args[1]);
                        var completionItem = await provider.ResolveCompletionItemAsync(editor.GetModel(), position, requestedItem);

                        if (completionItem != null)
                        {
                            return JsonConvert.SerializeObject(completionItem);
                        }
                    }

                    return null;
                });

                return editor.InvokeScriptAsync("registerCompletionItemProvider", new object[] { languageId, provider.TriggerCharacters }).AsAsyncAction();
            }

            return null;
        }

        public IAsyncAction RegisterHoverProviderAsync(string languageId, HoverProvider provider)
        {
            if (_editor.TryGetTarget(out CodeEditor editor))
            {
                // Wrapper around Hover Provider to Monaco editor.
                // TODO: Add Incremented Id so that we can register multiple providers per language?
                editor._parentAccessor.RegisterEvent("HoverProvider" + languageId, async (args) =>
                {
                    System.Diagnostics.Debug.WriteLine($"Hover provider.......... {args!=null}");
                    if (args != null && args.Length >= 1)
                    {
                        var hover = await provider.ProvideHover(editor.GetModel(), JsonConvert.DeserializeObject<Position>(args[0]));

                        if (hover != null)
                        {
                            return JsonConvert.SerializeObject(hover);
                        }
                    }

                    return string.Empty;
                });

                return editor.InvokeScriptAsync("registerHoverProvider", languageId).AsAsyncAction();
            }

            return null;
        }
    }
}
