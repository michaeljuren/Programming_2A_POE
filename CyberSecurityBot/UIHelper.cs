using System.Drawing;
using System.Windows.Forms;

namespace CyberSecurityBot
{
    public static class UIHelper
    {
        // ── Shared constants ──────────────────────────────────────────────────
        public static readonly Color BgMid      = Color.FromArgb(22,  27,  34);
        public static readonly Color AccentCyan = Color.FromArgb(0,  212, 212);
 
        private const string AsciiArt =
         	
            "                    ______  ____    ____  .______    _______ .______        .______     ______   .___________.  \r\n" +
            "                   /      ||\\   \\  /   / |   _  \\  |   ____||   _  \\       |   _  \\   /  __  \\  |           |  \r\n" +
            "                   |  ,----' \\   \\/   /  |  |_)  | |  |__   |  |_)  |      |  |_)  | |  |  |  | `---|  |----`  \r\n" +
            "                   |  |       \\_    _/   |   _  <  |   __|  |      /       |   _  <  |  |  |  |     |  |       \r\n" +
            "                   |  `----.    |  |     |  |_)  | |  |____ |  |\\  \\----. |  |_)  | |  `--'  |     |  |        \r\n" +
            "                   \\______|    |__|     |______/  |_______|| _| `._____| |______/   \\______/      |__|       ";


        private const string AsciiArtWithSubtitle = 
            AsciiArt +
            "\r\n\r\n" +
            "                                                 Cybersecurity Awareness Bot                                       ";
 
        /// <summary>
        /// Creates a read-only RichTextBox containing the ASCII banner.
        /// </summary>
        /// <param name="fontSize">Courier New font size in points.</param>
        /// <param name="includeSubtitle">Whether to append the subtitle row.</param>
        /// <param name="dock">DockStyle applied to the control.</param>
        public static RichTextBox CreateAsciiHeader(
            float fontSize,
            bool includeSubtitle = false,
            DockStyle dock = DockStyle.Fill)
        {
            string text = includeSubtitle ? AsciiArtWithSubtitle : AsciiArt;
            var font    = new Font("Courier New", fontSize, FontStyle.Bold);
 
            var rtb = new RichTextBox
            {
                Text        = text,
                Font        = new Font("Courier New", fontSize, FontStyle.Bold),
                ForeColor   = AccentCyan,
                BackColor   = BgMid,
                ReadOnly    = true,
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.None,
                WordWrap    = false,
                Dock        = dock,
                Margin      = new Padding(0),
                Padding     = new Padding(8, 6, 0, 0),
                TabStop     = false,
            };
 
            rtb.MouseDown += (_, _) => rtb.Select(0, 0);

            return rtb;
        }
    }
}   