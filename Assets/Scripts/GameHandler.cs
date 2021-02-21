using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameHandler : Singleton<GameHandler>
{

    [SerializeField] private Transform pf_Character;
    [SerializeField] private Transform pf_HitPartice;
    [SerializeField] private Transform pf_DiePartice;

    [SerializeField] private CharacterParametersSO playerSO;
    [SerializeField] private CharacterParametersSO enemySO;

    [SerializeField] private TMPro.TextMeshProUGUI scoreTextMesh;

    private List<Transform> _cache = new List<Transform>();

    [SerializeField] private PlayerControlHandler playerControl;

    public Transform[] startPositions;

    private int teamPlayerScore = 0;
    private int teamEnemyScore = 0;

    private void Start()
    {
        StartGame();
    }


    #region Utils funcs
    private void ClearScene()
    {

        for (int i = 0; i < _cache.Count; i++)
        {
            Destroy(_cache[i].gameObject);
        }

        _cache.Clear();

    }

    private int GetPosIndex(ref List<int> occupiedIndexes)
    {
        int indexPos = Random.Range(0, startPositions.Length);

        if (occupiedIndexes.Contains(indexPos)) 
            indexPos = GetPosIndex(ref occupiedIndexes);

        occupiedIndexes.Add(indexPos);

        return indexPos;
    }

    private Transform CreateCharacter(CharacterParametersSO charSO, Vector3 startPosition)
    {

        Transform GO = Instantiate(pf_Character);

        GO.position = startPosition;

        CharacterController characterController = GO.GetComponent<CharacterController>();

        characterController.Init(charSO);

        characterController.DieEvent += CharacterController_DieEvent;

        _cache.Add(GO);

        return GO;

    }

    private void CharacterController_DieEvent(int team)
    {

        if (team != 1)
            teamPlayerScore += 1;
        else
            teamEnemyScore += 1;

        string text = $"SCORE: {teamPlayerScore} - {teamEnemyScore}";

        scoreTextMesh.SetText(text); 
    }

    #endregion

    #region public funcs

    public void StartGame()
    {
        ClearScene();

        List<int> occupiedIndexes = new List<int>();

        int indexPos = GetPosIndex(ref occupiedIndexes);

        var Character = CreateCharacter(playerSO, startPositions[indexPos].position);
        Character.name = "Player";
        playerControl.Init(Character.GetComponent<CharacterController>());


        indexPos = GetPosIndex(ref occupiedIndexes);
        Character = CreateCharacter(enemySO, startPositions[indexPos].position);
        Character.name = "Enemy";
    }



    public void CreateHitParticleEffect(Vector3 position)
    {
        Transform GO = Instantiate(pf_HitPartice);

        GO.position = position;

        Destroy(GO.gameObject, 3f);
    }

    public void CreateDieParticleEffect(Vector3 position)
    {
        Transform GO = Instantiate(pf_DiePartice);

        GO.position = position;

        Destroy(GO.gameObject, 3f);
    }

    #endregion
}
