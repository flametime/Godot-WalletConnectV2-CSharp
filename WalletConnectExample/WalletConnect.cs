using System;
using Godot;
using QRCoder;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Storage;
using System.Drawing;

public class WalletConnect : Control
{
    [Export] private NodePath _walledAdressPath;
    private Label _walletAdress;

    [Export] private NodePath _deepLinkButtonPath;
    private Button _deepLinkButton;

    [Export] private NodePath _pcButtonPath;
    private Button _pcButton;

    [Export] private NodePath _qrTexturePath;
    private TextureRect _qrTexture;

    private SignClientOptions _dappOptions;
    private ConnectOptions _dappConnectOptions;
    
    public override void _Ready()
    {
        InitWalletConnect();

        _deepLinkButton = GetNode<Button>(_deepLinkButtonPath);
        _pcButton = GetNode<Button>(_pcButtonPath);
        _walletAdress = GetNode<Label>(_walledAdressPath);
        _qrTexture = GetNode<TextureRect>(_qrTexturePath);

        _deepLinkButton.Connect("button_down", this, nameof(ConnectMobile));
        _pcButton.Connect("button_down", this, nameof(ConnectPC));

    }

    private void InitWalletConnect()
    {
        _dappOptions = new SignClientOptions()
        {
            
            ProjectId = "024df4729317ed6a6fbd7cbe913f3f1c",
            Metadata = new Metadata()
            {
                Description = "Godot Demo",
                Icons = new[] { "https://upload.wikimedia.org/wikipedia/commons/thumb/6/6a/Godot_icon.svg/600px-Godot_icon.svg.png" },
                Name = "WalletConnectSharpv2 Godot Example",
                Url = "https://walletconnect.com"
            },
            // Uncomment to disable persistant storage
            Storage = new InMemoryStorage(),
            RelayUrl = "wss://relay.walletconnect.com"
        };

        _dappConnectOptions = new ConnectOptions()
        .RequireNamespace("eip155", new RequiredNamespace()
            .WithMethod("eth_sendTransaction")
            .WithMethod("eth_signTransaction")
            .WithMethod("eth_sign")
            .WithMethod("personal_sign")
            .WithMethod("eth_signTypedData")
            .WithChain("eip155:1")
            .WithEvent("chainChanged")
            .WithEvent("accountsChanged")
        );
    }

    private async void ConnectMobile()
    {
        GD.Print("!");

        var dappClient = await WalletConnectSignClient.Init(_dappOptions);

        GD.Print("!!");

        var connectData = await dappClient.Connect(_dappConnectOptions); 

        GD.Print("!!!");

        string url = "wc://wc?uri=" + Uri.EscapeDataString(connectData.Uri);

        OS.ShellOpen(url);

        GD.Print(url);

        SessionStruct session = await connectData.Approval;

        GD.Print(GetWalletAdress(session));

        _walletAdress.Text = GetWalletAdress(session);
    }

    private async void ConnectPC()
    {
        GD.Print("!");

        var dappClient = await WalletConnectSignClient.Init(_dappOptions);
        
        GD.Print("!!");

        var connectData = await dappClient.Connect(_dappConnectOptions); 

        GD.Print("!!!");

        ShowQR(connectData.Uri);

        SessionStruct session = await connectData.Approval;

        GD.Print(GetWalletAdress(session));
        
        _walletAdress.Text = GetWalletAdress(session);
    }

    private void ShowQR(string link)
    {
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.Q);
        PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);
    
        Godot.Image rawImg  = new Godot.Image();
        rawImg.LoadPngFromBuffer(qrCodeAsPngByteArr);

        ImageTexture texture = new ImageTexture();
        texture.CreateFromImage(rawImg);

        GD.Print(texture);

        _qrTexture.Texture = texture;

        GD.Print("QR generated");
    }

    private string GetWalletAdress(SessionStruct sessionData)
    {
        var selectedNamespace = sessionData.Namespaces["eip155"];

        if (selectedNamespace != null && selectedNamespace.Accounts.Length > 0)
        {
            var currentSession = selectedNamespace.Accounts[0];

            var parameters = currentSession.Split(':');

            return parameters[2];
        }
        
        return string.Empty;
    }

    
}