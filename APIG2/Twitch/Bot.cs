using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using APIG2.Messages;
using APIG2.Models.Json;
using CommunityToolkit.Mvvm.Messaging;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;
using YoutubeExplode.Videos;

namespace APIG2.Twitch;

public class Bot
{
    private readonly HttpClient _httpClient = new();
    private TwitchClient _twitchClient;
    private WebSocketClient _websocketClient;
    private bool couldConnect = false;
    private bool errored = false;

    public string Username { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public CustomReward? SelectedReward { get; set; }

    public Bot()
    {
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };
        _websocketClient = new WebSocketClient(clientOptions);
        _twitchClient = new TwitchClient(_websocketClient);
    }

    public async Task<bool> ConnectAsync()
    {
        if (_twitchClient.IsConnected)
            _twitchClient.Disconnect();
        if (_twitchClient.IsInitialized)
        {
            _websocketClient.Dispose();
            _websocketClient = new WebSocketClient(new ClientOptions()
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30),
                DisconnectWait = 500,
            });
            _twitchClient = new TwitchClient(_websocketClient);
        }

        _twitchClient.Initialize(new ConnectionCredentials(Username, Token), Username);

        _twitchClient.OnConnected += OnTwitchClientOnOnConnected;
        _twitchClient.OnConnectionError += OnTwitchClientOnOnConnectionError;
        _twitchClient.OnDisconnected += OnTwitchClientOnOnDisconnected;
        _twitchClient.OnMessageReceived += MessageReceived;
        _twitchClient.Connect();
        var startedWaiting = DateTime.Now;
        while (!couldConnect && !errored && (DateTime.Now - startedWaiting).TotalSeconds < 10)
            await Task.Delay(100);

        return !errored && couldConnect;
    }

    private void OnTwitchClientOnOnDisconnected(object? sender, OnDisconnectedEventArgs args)
    {
        Debug.WriteLine("Disconnected");
        _twitchClient.OnConnected -= OnTwitchClientOnOnConnected;
        _twitchClient.OnConnectionError -= OnTwitchClientOnOnConnectionError;
        _twitchClient.OnDisconnected -= OnTwitchClientOnOnDisconnected;
        _twitchClient.OnMessageReceived -= MessageReceived;
        couldConnect = false;
        errored = false;
        App.Messenger.Send(new TwitchBotConnectionMessage(TwitchBotConnectionStatus.Disconnected, "Disconnected"));
    }

    private void OnTwitchClientOnOnConnectionError(object? sender, OnConnectionErrorArgs args)
    {
        Debug.WriteLine("Connection error");
        Debug.WriteLine(args.Error.Message);
        App.Messenger.Send(new TwitchBotConnectionMessage(TwitchBotConnectionStatus.Error, args.Error.Message));
        errored = true;
    }

    private void OnTwitchClientOnOnConnected(object? sender, OnConnectedArgs args)
    {
        Debug.WriteLine("Connected");
        App.Messenger.Send(new TwitchBotConnectionMessage(TwitchBotConnectionStatus.Connected, "Connected"));
        couldConnect = true;
    }

    private void MessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        if (SelectedReward == null)
            return;

        if (e.ChatMessage.CustomRewardId != SelectedReward.Id)
            return;
        
        var message = e.ChatMessage.Message;
        var isVideo = VideoId.TryParse(message);

        if (isVideo == null)
            return;
        
        App.Messenger.Send(new TwitchBotRequestReceived($"https://www.youtube.com/watch?v={isVideo.Value}"));
    }

    public async Task<TwitchRewardsResponse.Root?> GetChannelPointRewardsAsync(string channelName)
    {
        var payload = $$"""
                      [
                        {
                          "operationName": "ChannelPointsContext",
                          "variables": {
                            "channelLogin": "{{channelName}}"
                          },
                          "extensions": {
                            "persistedQuery": {
                              "version": 1,
                              "sha256Hash": "1530a003a7d374b0380b79db0be0534f30ff46e61cffa2bc0e2468a909fbc024"
                            }
                          }
                        }
                      ]
                      """;

        using var msg = new HttpRequestMessage(HttpMethod.Post, "https://gql.twitch.tv/gql")
        {
            Content = new StringContent(payload)
        };
        msg.Headers.Add("Client-Id", "kimne78kx3ncx6brgo4mv6wki5h1ko");

        var response = await _httpClient.SendAsync(msg);
        if (!response.IsSuccessStatusCode)
            return default;

        var asString = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize(asString, TwichRewardsResponseContext.Default.RootArray);
        return data?[0];
    }

    public void Disconnect()
    {
        if (_twitchClient.IsConnected)
            _twitchClient.Disconnect();
        _websocketClient.Close();
    }
}