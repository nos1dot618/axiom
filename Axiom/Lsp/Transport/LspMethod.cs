namespace Axiom.Lsp.Transport;

public static class LspMethod
{
    public static class Request
    {
        public const string Initialize = "initialize";
        public const string TextCompletion = "textDocument/completion";
    }

    public static class Notification
    {
        public const string Initialized = "initialized";
        public const string Shutdown = "shutdown";
        public const string Exit = "exit";

        public const string DidOpen = "textDocument/didOpen";
        public const string DidChange = "textDocument/didChange";
        public const string DidSave = "textDocument/didSave";
    }
}