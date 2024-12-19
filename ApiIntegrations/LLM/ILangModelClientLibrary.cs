using CommonTypes;

namespace ApiIntegrations.LLM
{
	public delegate void ProcessReceivedSentenceStream(string intermediateResponse, int sequenceNumber);
    public delegate void ProcessReceivedAudioStream(string base64Audio, int sequenceNumber);

    public interface ILangModelClientLibrary
    {
        Task<string> MakeApiRequest(List<Message> messages, string model);
        Task<string> MakeFunctionCallApiRequest(List<Message> messages, string functionDefinitions, string functionToInvoke, string model);
        Task<string> MakeStreamingApiRequest(List<Message> messages, ProcessReceivedSentenceStream processSentence, string model, string avatarName);
    }
}
