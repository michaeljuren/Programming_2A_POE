using System.Media;

namespace CyberSecurityBot
{
    public static class AudioPlayer
    {

        public static void PlayWelcome()
        {
            try
            {
               SoundPlayer player = new("welcome.wav");
               player.PlaySync();
            }
            catch
            {
                // Fail gracefully if audio missing
            }
        }
    }
}