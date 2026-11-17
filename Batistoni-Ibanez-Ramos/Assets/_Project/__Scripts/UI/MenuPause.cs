using PatronesAplicados;
using PatronesAplicados.RealImplementation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuPause : MonoBehaviour
{
    [Header("Referencias del Menú de Pausa")]
    [Tooltip("Arrastra aquí el objeto 'Canvas Pause' (o déjalo vacío y se detectará solo).")]
    [SerializeField] private GameObject menuPausaCanvas;

    [Tooltip("Arrastra aquí el panel 'Options' (NO el botón Options).")]
    [SerializeField] private GameObject optionsMenu;

    [Tooltip("Arrastra aquí el panel 'Controls' (NO el botón Controls).")]
    [SerializeField] private GameObject controlsMenu;

    private GameObject _rootPauseObject;
    private GameObject _panelPauseButtons;
    private bool _hasRequestedPause;

    public static int LastToggleFrame = -1;

    private void Awake()
    {
        ResolveHierarchyReferences();
    }

    private void ResolveHierarchyReferences()
    {
        if (_rootPauseObject == null)
        {
            Transform current = transform;
            while (current != null)
            {
                if (current.name.IndexOf("Canvas Pause", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    current.name.IndexOf("CanvasPause", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _rootPauseObject = current.gameObject;
                    break;
                }

                if (current.parent != null && current.parent.name.Equals("CANVAS", System.StringComparison.OrdinalIgnoreCase))
                {
                    _rootPauseObject = current.gameObject;
                    break;
                }
                current = current.parent;
            }

            if (_rootPauseObject == null)
            {
                _rootPauseObject = menuPausaCanvas != null ? menuPausaCanvas : gameObject;
            }
        }

        Transform searchRoot = _rootPauseObject.transform;

        if (_panelPauseButtons == null)
        {
            Transform foundPanel = searchRoot.Find("Panel Pause");
            if (foundPanel != null) _panelPauseButtons = foundPanel.gameObject;
        }

        if (optionsMenu == null || IsInsideMainButtonsPanel(optionsMenu.transform))
        {
            Transform foundOptions = searchRoot.Find("Options");
            if (foundOptions != null) optionsMenu = foundOptions.gameObject;
        }

        if (controlsMenu == null || IsInsideMainButtonsPanel(controlsMenu.transform))
        {
            Transform foundControls = searchRoot.Find("Controls");
            if (foundControls != null) controlsMenu = foundControls.gameObject;
        }
    }

    private bool IsInsideMainButtonsPanel(Transform target)
    {
        if (target == null) return false;

        if (target.GetComponent<UnityEngine.UI.Button>() != null) return true;
        if (_panelPauseButtons != null && target.IsChildOf(_panelPauseButtons.transform) && target != _panelPauseButtons.transform)
        {
            return true;
        }
        return false;
    }

    private void OnEnable()
    {
        ResolveHierarchyReferences();
        RestoreMainPausePanel();

        if (optionsMenu != null && !IsInsideMainButtonsPanel(optionsMenu.transform))
            optionsMenu.SetActive(false);

        if (controlsMenu != null && !IsInsideMainButtonsPanel(controlsMenu.transform))
            controlsMenu.SetActive(false);

        RequestPauseOnce();
    }

    private void Update()
    {
        if (LastToggleFrame == Time.frameCount) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            LastToggleFrame = Time.frameCount;
            HandleEscapePressed();
        }
    }

    public void HandleEscapePressed()
    {
        ResolveHierarchyReferences();

        bool subMenuOpen = (optionsMenu != null && !IsInsideMainButtonsPanel(optionsMenu.transform) && optionsMenu.activeInHierarchy) ||
                           (controlsMenu != null && !IsInsideMainButtonsPanel(controlsMenu.transform) && controlsMenu.activeInHierarchy);

        if (subMenuOpen)
        {
            if (optionsMenu != null && !IsInsideMainButtonsPanel(optionsMenu.transform))
                optionsMenu.SetActive(false);

            if (controlsMenu != null && !IsInsideMainButtonsPanel(controlsMenu.transform))
                controlsMenu.SetActive(false);

            RestoreMainPausePanel();
            return;
        }

        ResumeGame();
    }

    private void OnDisable()
    {
        ReleasePauseOnce();
    }

    public void RestoreMainPausePanel()
    {
        if (_panelPauseButtons != null)
        {
            _panelPauseButtons.SetActive(true);

            for (int i = 0; i < _panelPauseButtons.transform.childCount; i++)
            {
                _panelPauseButtons.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        else if (menuPausaCanvas != null)
        {
            menuPausaCanvas.SetActive(true);
        }
    }

    public void RequestPauseOnce()
    {
        if (_hasRequestedPause) return;
        _hasRequestedPause = true;

        PauseManager.RequestPause();
    }

    public void ReleasePauseOnce()
    {
        if (!_hasRequestedPause) return;
        _hasRequestedPause = false;

        PauseManager.RequestResume();
    }

    public void ResumeGame()
    {
        ResolveHierarchyReferences();

        if (optionsMenu != null && !IsInsideMainButtonsPanel(optionsMenu.transform))
            optionsMenu.SetActive(false);

        if (controlsMenu != null && !IsInsideMainButtonsPanel(controlsMenu.transform))
            controlsMenu.SetActive(false);

        RestoreMainPausePanel();
        ReleasePauseOnce();

        if (_rootPauseObject != null)
        {
            _rootPauseObject.SetActive(false);
        }
        else if (menuPausaCanvas != null)
        {
            menuPausaCanvas.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void ChangeScene()
    {
        _hasRequestedPause = false;
        PauseManager.ClearAll();
        DamageableRegistry.Clear();
        SceneManager.LoadScene("Main Menu");
    }
}