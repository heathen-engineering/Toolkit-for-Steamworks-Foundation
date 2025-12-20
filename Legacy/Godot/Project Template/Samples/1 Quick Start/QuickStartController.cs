using Godot;
using HeathenEngineering.SteamworksIntegration;
using System;

namespace HeathenEngineering.Samples
{
    public class QuickStartController : Node
    {
        [Export]
        public NodePath avatarImage;
        [Export]
        public NodePath userName;

        public override void _Ready()
        {
            /************************************************************************************************
             * 
             * The Heathen/Steamworks plugin has established and AutoLoader script SteamworksBehaviour
             * 
             * The Steamworks Behaviour is responsible for configuraing and initalizing the Steam API
             * By default it is configured for App 480 aka Spacewars, you will need to update the App Id
             * In that script with your own App ID when you get one.
             * 
             ***********************************************************************************************/

            //Grab a reference to the TextureRect where we will show the user's avatar image
            var avatar = GetNode<TextureRect>(avatarImage);
            //Grab a reference to the RichTextLabel where we will show the user's name
            var name = GetNode<RichTextLabel>(userName);

            /************************************************************************************************
             *
             * UserData is a struct that wraps the CSteamID and exposes common UserData related methods for you
             * In this case we are using the Me field to get the local user's UserData
             * We then call LoadAvatar to download and import the avatar image from Valve's Steam Client
             * 
             * Once complete the callback is raised and we use that Image to create a ImageTexture for use
             * on our TextureRect
             * 
             * Similarly we use the Me field to get the local user and its name to populate the RichTextLabel
             * 
             * This simply demonstrates a fundamental use of Steam (getting at user data) and integrating it 
             * with Godot (converting the byte[] from Steam to an Image for Godot. You can do so much more 
             * using Heathen's Toolkit or you can use the raw Steam API via the integrated Steamworks.NET
             * also included.
             *
             ***********************************************************************************************/
            UserData.Me.LoadAvatar(image =>
            {
                var imageTexture = new ImageTexture();
                imageTexture.CreateFromImage(image);
                avatar.Texture = imageTexture;
                name.Text = UserData.Me.Name;
            });
        }

        public void OnKBPressed()
        {
            GD.Print("Opening the Heathen Knowledge Base!");
            OS.ShellOpen("https://kb.heathenengineering.com/assets/steamworks");
        }

        public void OnCommunityPressed()
        {
            GD.Print("Joining the Heathen Community!");
            OS.ShellOpen("https://discord.gg/6X3xrRc");
        }

        public void OnSponsorPressed()
        {
            GD.Print("Opening GitHub Sponsors ... Thank You!");
            OS.ShellOpen("https://github.com/sponsors/heathen-engineering");
        }
    }
}
