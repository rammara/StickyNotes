using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace StickyNotes.Services
{
    public class NamedPipeService : IDisposable
    {
        private const string PipeName = "StickyNotesPipe-b7d1069f-5ac1-4aaa-83d7-f5c5d3c363b8";
        private const string SecretKey = "Rammara-f853edc6-58ca-4d20-ac38-f30ba67a0752";
        private NamedPipeServerStream? _pipeServer;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isDisposed;

        public event EventHandler<string>? DataReceived;
        public const int TIMEOUT = 2000;

        public void StartServer()
        {
            if (_pipeServer != null)
                return;

            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            _pipeServer = new NamedPipeServerStream(
                                PipeName,
                                PipeDirection.InOut,
                                NamedPipeServerStream.MaxAllowedServerInstances,
                                PipeTransmissionMode.Message,
                                PipeOptions.Asynchronous,
                                4096,
                                4096);
                            if (_pipeServer is null) throw new Exception("Could not create a pipe server.");

                            await _pipeServer.WaitForConnectionAsync(_cancellationTokenSource.Token);
                            var connectedPipe = _pipeServer;
                            _ = Task.Run(() => HandleConnection(connectedPipe), _cancellationTokenSource.Token);
                        } // try
                        catch (OperationCanceledException)
                        {
                            break;
                        } // catch
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"NamedPipeService error: {ex.Message}");
                            await Task.Delay(1000, _cancellationTokenSource.Token);
                        } // catch 
                        _pipeServer = null;
                    } // while
                } // try
                catch (Exception ex)
                {
                    Debug.WriteLine($"Critical error in NamedPipeService: {ex.Message}");
                } // catch
            }, _cancellationTokenSource.Token);
        } // StartServer

        public const string READY_MSG = "RDY";
        public const string ACK_MSG = "ACK";

        private async Task HandleConnection(NamedPipeServerStream pipeServer)
        {
            try
            {
                if (!pipeServer.IsConnected)
                {
                    Debug.WriteLine("Pipe is not connected in HandleConnection");
                    return;
                }

                using var reader = new StreamReader(pipeServer, Encoding.UTF8, leaveOpen: true);
                using var writer = new StreamWriter(pipeServer, Encoding.UTF8, leaveOpen: true);
                writer.AutoFlush = true;
                await writer.WriteLineAsync(READY_MSG);
                var data = await reader.ReadLineAsync();

                if (string.IsNullOrEmpty(data))
                {
                    await writer.WriteLineAsync("ERROR: Empty data");
                    return;
                }
                if (!data.StartsWith(SecretKey))
                {
                    await writer.WriteLineAsync("ERROR: Invalid key");
                    return;
                }
                var actualData = data[SecretKey.Length..];
                await writer.WriteLineAsync(ACK_MSG);
                DataReceived?.Invoke(this, actualData);
            } // try
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling the connection: {ex.Message}");
            } // catch
            finally
            {
                try
                {
                    pipeServer.Close();
                    pipeServer.Dispose();
                } // try
                catch
                {
                    // Ignore
                } // catch
            } // finally
        } // HandleConnection

        public static async Task<bool> SendToExistingInstance(string[] args)
        {
            NamedPipeClientStream? pipeClient = null;
            try
            {
                pipeClient = new NamedPipeClientStream(
                    ".",
                    PipeName,
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous);

                await pipeClient.ConnectAsync(TIMEOUT);
                if (!pipeClient.IsConnected)
                {
                    return false;
                }
                using (pipeClient)
                using (var reader = new StreamReader(pipeClient, Encoding.UTF8))
                using (var writer = new StreamWriter(pipeClient, Encoding.UTF8))
                {
                    writer.AutoFlush = true;
                    var readyMessage = await reader.ReadLineAsync();
                    if (readyMessage != READY_MSG)
                    {
                        return false;
                    }
                    var data = string.Join(" ", args);
                    var securedData = SecretKey + data;
                    await writer.WriteLineAsync(securedData);
                    var response = await reader.ReadLineAsync();
                    return response == ACK_MSG;
                } // using 
            } // try
            catch (TimeoutException)
            {
                pipeClient?.Dispose();
                return false;
            } // catch
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending data to the existing instance: {ex.Message}");
                pipeClient?.Dispose();
                return false;
            } // catch
        } // SendToExistingInstance

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();

                _pipeServer?.Close();
                _pipeServer?.Dispose();
            } // Try
            catch
            {
                // Ignore
            } // catch

            GC.SuppressFinalize(this);
        } // Dispose
    } // class NamedPipeService
} // namespace