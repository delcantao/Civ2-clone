using Civ2.Dialogs.FileDialogs;
using Civ2.Dialogs.NewGame;
using Civ2.Rules;
using Civ2engine;
using Model;
using Model.InterfaceActions;

namespace Civ2.Dialogs;

public class MainMenu : BaseDialogHandler
{
    public const string Title = "MAINMENU";
    public MainMenu() : base(Title, -0.08, -0.07) { }

    public override IInterfaceAction HandleDialogResult(DialogResult result,
        Dictionary<string, ICivDialogHandler> civDialogHandlers, Civ2Interface civ2Interface)

    {
        if (result.SelectedButton == Dialog.Dialog.Button[1])
        {
            return civ2Interface.InitialMenu != Title ? civ2Interface.GetInitialAction() : ExitAction.Exit;
        }

        switch (result.SelectedIndex)
        {
            case 0:
            case 2:
                Initialization.ConfigObject.CustomizeWorld = result.SelectedIndex == 2;
                if (civ2Interface.MainApp.AllRuleSets.Length > 1)
                    return civDialogHandlers[SelectGameVersionHandler.Title].Show(civ2Interface);
                civ2Interface.MainApp.SetActiveRuleSet(0);
                Initialization.LoadGraphicsAssets(civ2Interface);
                return civDialogHandlers[WorldSizeHandler.Title].Show(civ2Interface);


            case 1:
                 return civDialogHandlers[LoadMap.DialogTitle].Show(civ2Interface);
            case 3:
                Initialization.ConfigObject.IsScenario = true;
                return civDialogHandlers[LoadScenario.DialogTitle].Show(civ2Interface);
            case 4:
                return civDialogHandlers[LoadGame.DialogTitle].Show(civ2Interface);
        }
        return new MenuAction(Dialog);
    }
}