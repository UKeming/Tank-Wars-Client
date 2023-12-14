// @File: TankWarsWin.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/06
// @Author: Keming Chen, Yifei Sun

using System.Drawing;
using System.Windows.Forms;

namespace View
{
    /// <summary>
    ///     The game window.
    /// </summary>
    public partial class TankWarsWin : Form
    {
        // Delegate for drawer methods.
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        // The address box.
        private readonly TextBox addressBox;

        // The connect button.
        private readonly Button connectButton;

        // The drawingPanel on window.
        private readonly DrawingPanel drawingPanel;

        // The name box.
        private readonly TextBox nameBox;

        /// <summary>
        ///     Initialize window, create components.
        /// </summary>
        public TankWarsWin()
        {
            InitializeComponent();
            // Add name label.
            var nameLabel = new Label();
            nameLabel.Text = "name:";
            nameLabel.TextAlign = ContentAlignment.MiddleRight;
            nameLabel.Location = new Point(15, 5);
            nameLabel.Width = 60;
            Controls.Add(nameLabel);

            // Add name box.
            nameBox = new TextBox();
            nameBox.Text = "player";
            nameBox.Location = new Point(80, 5);
            Controls.Add(nameBox);

            // Add address label.
            var addressLabel = new Label();
            addressLabel.Text = "address:";
            addressLabel.TextAlign = ContentAlignment.MiddleRight;
            addressLabel.Location = new Point(200, 5);
            addressLabel.Width = 60;
            Controls.Add(addressLabel);

            // Add address box.
            addressBox = new TextBox();
            addressBox.Location = new Point(265, 5);
            Controls.Add(addressBox);
            addressBox.Text = "localhost";

            // Add connnect button.
            connectButton = new Button();
            connectButton.Text = "Connect";
            connectButton.Location = new Point(385, 5);
            connectButton.MouseClick += OnConnect;
            Controls.Add(connectButton);

            // Add drawingPanel.
            drawingPanel = new DrawingPanel();
            drawingPanel.Location = new Point(0, 30);
            drawingPanel.Size = new Size(Width, Height - 30);
            Controls.Add(drawingPanel);

            // Register events.
            drawingPanel.MouseMove += DrawingPanel_MouseMove;
            drawingPanel.MouseClick += DrawingPanel_MouseClick;
            KeyPreview = true;
            KeyDown += TankWarsWin_KeyDown;
            KeyUp += TankWarsWin_KeyUp;
            drawingPanel.controller.ErrorArrived += OnError;
        }

        /// <summary>
        ///     When keyup, stop the tank.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TankWarsWin_KeyUp(object sender, KeyEventArgs e)
        {
            drawingPanel.controller.StopPlayer(e.KeyCode);
        }

        /// <summary>
        ///     Use key to control the movement of tank.
        ///     A: left, W: up, R: right, D: down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TankWarsWin_KeyDown(object sender, KeyEventArgs e)
        {
            drawingPanel.controller.MovePlayer(e.KeyCode);
            if (drawingPanel.controller.gameHasStarted())
                e.SuppressKeyPress = e.Handled = true;
        }

        /// <summary>
        ///     Control shooting.
        ///     Left click is shoot, right click is beam attack.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawingPanel_MouseClick(object sender, MouseEventArgs e)
        {
            drawingPanel.controller.Fire(e.Button);
        }

        /// <summary>
        ///     Control the target of shoot using mouse direction.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            drawingPanel.controller.Target(e.X - drawingPanel.Width / 2, e.Y - drawingPanel.Height / 2);
        }

        /// <summary>
        ///     Handle error if error occurs when connecting/receiving from server.
        /// </summary>
        /// <param name="message"></param>
        private void OnError(string message)
        {
            MessageBox.Show(message);
            Invoke(new MethodInvoker(() =>
            {
                connectButton.Enabled = true;
                addressBox.Enabled = true;
                nameBox.Enabled = true;
            }));
        }

        /// <summary>
        ///     Connect to server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnect(object sender, MouseEventArgs e)
        {
            drawingPanel.controller.ConnectToServer(addressBox.Text, 11000, nameBox.Text);
            connectButton.Enabled = false;
            addressBox.Enabled = false;
            nameBox.Enabled = false;
        }
    }
}