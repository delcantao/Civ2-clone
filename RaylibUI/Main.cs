﻿using Raylib_cs;
using System.Numerics;
using Civ2engine;
using Civ2engine.MapObjects;
using Model;
using RaylibUI.Initialization;
using RaylibUI.Forms;
using JetBrains.Annotations;

namespace RaylibUI
{
    public partial class Main
    {
        private Map map;

        private IScreen _activeScreen;
        private bool _shouldClose;


        internal readonly Sound Soundman;

        public Main()
        {
            var hasCivDir = Settings.LoadConfigSettings();

            //========= RAYLIB WINDOW SETTINGS
            Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_VSYNC_HINT |
                                  ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.InitWindow(1280, 800, "raylib - civ2");
            //Raylib.SetTargetFPS(60);
            Raylib.InitAudioDevice();
            Soundman = new Sound();

            //========== IMGUI STYLE

            Raylib.SetExitKey(KeyboardKey.KEY_F12);

            //============ LOAD REQUIRED SAV GAME DATA
            if (hasCivDir)
            {
                _activeScreen = SetupMainScreen();
            }
            else
            {
                _activeScreen = new GameFileLocatorScreen(() =>
                {
                    hasCivDir = true;
                    _activeScreen = SetupMainScreen();
                });
            }

            //============ LOAD SOUNDS

            //prep this for a loop( should split that function out between loops and non loops)

            //play a sound
            //soundman.PlayCIV2DefaultSound("DIVEBOMB");

            FormManager.Initialize();
        }

        public void RunLoop()
        {
            var counter = 0;
            var pulse = false;

            while (!Raylib.WindowShouldClose() && !_shouldClose)
            {

                Raylib.BeginDrawing();

                int screenHeight = Raylib.GetScreenHeight();

                _activeScreen.Draw(pulse);

                Raylib.DrawText($"{Raylib.GetFPS()} FPS", 5, screenHeight - 20, 20, Color.BLACK);

                Raylib.EndDrawing();
                if (counter++ >= 30)
                {
                    pulse = !pulse;
                    counter = 0;
                }
            }

            ShutdownApp();
        }

        private MainMenu SetupMainScreen()
        {                
            Labels.UpdateLabels(null); 
            Interfaces = Helpers.LoadInterfaces();

            ActiveInterface = Helpers.GetInterface(Settings.Civ2Path, Interfaces);
            return new MainMenu(this,() => _shouldClose= true, StartGame, Soundman);
        }


        public IUserInterface ActiveInterface { get; set; }

        public IList<IUserInterface> Interfaces { get; set; }

        void ShutdownApp()
        {
            Soundman.Dispose();
            Raylib.CloseWindow();
            Raylib.CloseAudioDevice();
        }
    }
}
