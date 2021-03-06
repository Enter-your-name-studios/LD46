﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using TMPro;

public class Player : MonoBehaviour {
	const string SAVE_PROGRESS_KEY = "Difficulty";

	public PlayerScreenState ScreenState { get; set; } = PlayerScreenState.None;

	[Header("Minigames")]
	[SerializeField] [ReorderableList] List<BaseBaseMinigame> minigames = null;
	[SerializeField] [ReorderableList] List<MinigameLauncher> minigamesSingle = null;

	[Header("Audio")]
	[SerializeField] AudioClip mainTheme = null;

	[Header("Refs")]
	[SerializeField] GameMenu gameMenu = null;
	[SerializeField] TextMeshProUGUI debugTextField = null;

	BaseBaseMinigame currMinigame = null;
	byte currMinigameId = 0;
	byte currDifficulty = 0;

	private void Awake() {
		currDifficulty = (byte)PlayerPrefs.GetInt(SAVE_PROGRESS_KEY, 0);
		debugTextField.text = $"Difficulty: {currDifficulty}";
	}

	private void Start() {
		for(byte i = 0; i < currDifficulty; ++i) {
			if(i < minigamesSingle.Count)
				minigamesSingle[i].Enable();
		}

		if (mainTheme != null) {
			LeanTween.delayedCall(0.1f, () => {
				AudioManager.Instance.PlayLoop(mainTheme, 0.7f, channel: AudioManager.AudioChannel.Music);
			});
		}
	}

	void Update() {
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			debugTextField.gameObject.SetActive(!debugTextField.gameObject.activeSelf);
		}
#endif

		if (ScreenState == PlayerScreenState.Cinematic && Input.anyKeyDown) {
			gameMenu.ShowMainMenu();
		}
		else if (Input.GetKeyDown(KeyCode.Escape)) {
			if (ScreenState == PlayerScreenState.Minigame) {
				currMinigame.Uninit();
				currMinigame = null;
				OnEndSingle();
			}
			else if (ScreenState == PlayerScreenState.SubMainMenu) {
				gameMenu.OnCreditsBackClick();
			}
			else if (ScreenState == PlayerScreenState.MainMenu) {
				gameMenu.OnExitClick();
			}
		}
	}

	public void StartLoop() {
		ScreenState = PlayerScreenState.Minigame;
		gameMenu.HideMainMenu();
		StartNewMinigameInSequence();
	}
	
	public void PlayMinigame(MinigameType type) {
		BaseBaseMinigame minigame = null;
		for(byte i = 0; i < minigames.Count; ++i) {
			if(minigames[i].type == type) {
				minigame = minigames[i];
				break;
			}
		}

		if(minigame != null) {
			ScreenState = PlayerScreenState.Minigame;
			gameMenu.HideMainMenu();
			StartSingleMinigame(minigame);
		}
	}

	void StartNewMinigameInSequence() {
		Debug.Log($"Start new minigame. Id: {currMinigameId}");
		currMinigame = Instantiate(minigames[currMinigameId].gameObject, transform).GetComponent<BaseBaseMinigame>();

		currMinigame.onWinEvent += OnWinMinigameInSequence;
		currMinigame.onLoseEvent += OnLoseMinigameInSequence;

		currMinigame.Init(currDifficulty);
	}

	void OnWinMinigameInSequence() {
		++currMinigameId;
		if (currMinigameId == minigames.Count) {
			OnEndSequence();
			return;
		}

		StartNewMinigameInSequence();
	}

	void OnLoseMinigameInSequence() {
		++currMinigameId;
		if (currMinigameId == minigames.Count) {
			OnEndSequence();
			return;
		}

		StartNewMinigameInSequence();
	}

	void OnEndSequence() {
		OnEndSingle();

		IncreaseDifficulty();
		if (currDifficulty == minigamesSingle.Count)
			gameMenu.PlayEndAnimation();
	}

	void StartSingleMinigame(BaseBaseMinigame minigame) {
		Debug.Log($"Start new minigame. Single: {minigame.transform.name}");
		currMinigame = Instantiate(minigame.gameObject, transform).GetComponent<BaseBaseMinigame>();

		currMinigame.onWinEvent += OnEndSingle;
		currMinigame.onLoseEvent += OnEndSingle;

		currMinigame.Init(currDifficulty);
	}

	void OnEndSingle() {
		if(currMinigameId != 0) {
			minigames.Shuffle();
			currMinigameId = 0;
		}

		gameMenu.ShowMainMenu();
	}

	#region CHEATS
	public void OnClearDifficultyClick() {
		ResetDifficulty();
	}

	public void OnAddDifficultyClick() {
		IncreaseDifficulty();
	}

	void IncreaseDifficulty() {
		if (currDifficulty < minigamesSingle.Count)
			minigamesSingle[currDifficulty].Enable();
		++currDifficulty;
		PlayerPrefs.SetInt(SAVE_PROGRESS_KEY, currDifficulty);
		debugTextField.text = $"Difficulty: {currDifficulty}";
	}

	void ResetDifficulty() {
		currDifficulty = 0;
		PlayerPrefs.SetInt(SAVE_PROGRESS_KEY, currDifficulty);
		debugTextField.text = $"Difficulty: {currDifficulty}";

		for (byte i = 0; i < minigamesSingle.Count; ++i) {
			minigamesSingle[i].Disable();
		}
	}

	#endregion
}
