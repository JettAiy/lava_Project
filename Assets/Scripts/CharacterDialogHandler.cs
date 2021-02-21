using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDialogHandler : MonoBehaviour
{

    private Transform canvas;
    private Transform camera;

    [SerializeField] private TMPro.TextMeshProUGUI dialogTextMesh;

    [SerializeField] private List<string> dialogs;

    private float dialogTime;

    private bool showDialogs;

    private void Start()
    {
        canvas = GetComponentInChildren<Canvas>().transform;
        camera = Camera.main.transform;
        SetDialogTime();

        CharacterController characterController = GetComponent<CharacterController>();

        characterController.DieEvent += CharacterController_DieEvent;

        showDialogs = characterController.parametersSO.team != 1;
        ShowDialog(true);
    }

    private void CharacterController_DieEvent(int team)
    {
        showDialogs = false;

        string text = "...>_<...";

        if (team == 1)
        {
            text = "this game sucks...";
        }

        dialogTextMesh.SetText(text);
    }

    private void SetDialogTime()
    {
        dialogTime = Random.Range(10f, 20f);
    }

    private void ShowDialog(bool clear = false)
    {
        if (clear)
        {
            dialogTextMesh.SetText("");
        }
        else
        {
            dialogTextMesh.SetText(dialogs[Random.Range(0, dialogs.Count)]);
        }
        
    }

    private void Update()
    {

        canvas.LookAt(transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);

        if (!showDialogs) return;

        dialogTime -= Time.deltaTime;

        if (dialogTime <= 0)
        {
            SetDialogTime();
            ShowDialog();
        }

        if (dialogTime >2f && dialogTime < 3f)
        {
            ShowDialog(true); 
        }
    }

}
