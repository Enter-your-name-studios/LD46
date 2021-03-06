﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PandaMinigame : BaseMinigame {
	byte matchs = 0;
	byte wrongMatchs = 0;

	[Header("Refs")]
	[SerializeField] Transform centerPanda = null;
	[SerializeField] Transform leftPanda = null;
	[SerializeField] Transform rightPanda = null;
	[SerializeField] GameObject[] health = null;
	[SerializeField] ParticleSystem winParticles = null;
	[SerializeField] ParticleSystem loseParticles = null;

	[SerializeField] SpriteRendererAnimator[] genders = null;

	[Header("Debug")]
	[SerializeField] TextMeshProUGUI debugTextField = null;

	[Header("Animations")]
	[SerializeField] GameObject minigame;
	[SerializeField] GameObject winAnimation;
	[SerializeField] GameObject loseAnimation;

	Vector3 centerPos;
	Vector3 leftPos;
	Vector3 rightPos;
	Vector3 leftScale;
	Vector3 rightScale;

	bool centerGender;
	bool leftGender;
	bool rightGender;

	PandaMinigameDifficulty difficulty;

	public override void Init(byte usedDIfficulty) {
		base.Init(usedDIfficulty);
		difficulty = difficultyBase as PandaMinigameDifficulty;
		matchs = difficulty.matchs;

		centerPos = centerPanda.position;
		leftPos = leftPanda.position;
		rightPos = rightPanda.position;
		leftScale = leftPanda.localScale;
		rightScale = rightPanda.localScale;
		debugTextField.enabled = false;

		for (byte i = difficulty.maxWrongMatchs; i < health.Length; ++i) {
			health[i].SetActive(false);
		}

		SpawnNewPanda();
	}

	protected new void Update() {
		base.Update();

#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			debugTextField.enabled = !debugTextField.enabled;
		}
#endif
	}

	public void LeftClick() {
		if (leftGender == centerGender)
			WrongMatch();
		else
			RightMatch();
	}

	public void RightClick() {
		if (rightGender == centerGender)
			WrongMatch();
		else
			RightMatch();
	}

	void RightMatch() {
		--matchs;

		loseParticles.Stop();
		winParticles.Play();
		LeanTween.cancel(winParticles.gameObject, false);
		LeanTween.delayedCall(winParticles.gameObject, 0.1f, () => winParticles.Stop());

		debugTextField.text = $"Prev math right.";

		if (matchs <= 0) {
			isPlaying = false;
			ShowWinAnimation();
		}
		else {
			SpawnNewPanda();
		}
	}

	void WrongMatch() {
		--matchs;

		if(difficulty.maxWrongMatchs != 0)
			health[difficulty.maxWrongMatchs - 1 - wrongMatchs].SetActive(false);
		++wrongMatchs;

		winParticles.Stop();
		loseParticles.Play();
		LeanTween.cancel(loseParticles.gameObject, false);
		LeanTween.delayedCall(loseParticles.gameObject, 0.1f, () => loseParticles.Stop());

		debugTextField.text = $"Prev math wrong.";

		if (wrongMatchs >= difficulty.maxWrongMatchs) {
			isPlaying = false;
			ShowLoseAnimation();
		}
		else {
			SpawnNewPanda();
		}
	}

	void SpawnNewPanda() {
		debugTextField.text += $" Total: {matchs} WrongLeft: {wrongMatchs}";

		centerGender = Random.Range(0, 2) == 0;
		if (difficulty.randomLeftRight) {
			leftGender = Random.Range(0, 2) == 0;
			rightGender = !leftGender;
		}
		else {
			leftGender = false;
			rightGender = true;
		}

		Destroy(centerPanda.gameObject);
		Destroy(leftPanda.gameObject);
		Destroy(rightPanda.gameObject);

		centerPanda = Instantiate(genders[centerGender ? 1 : 0], centerPos, Quaternion.identity, minigame.transform).transform;
		leftPanda = Instantiate(genders[leftGender ? 1 : 0], leftPos, Quaternion.identity, minigame.transform).transform;
		rightPanda = Instantiate(genders[rightGender ? 1 : 0], rightPos, Quaternion.identity, minigame.transform).transform;

		//centerPanda.transform.localScale = new Vector3(centerGender != rightGender ? 1 : -1, 1, 1);
		leftPanda.localScale = leftScale;
		rightPanda.localScale = rightScale;
	}

	protected override void ShowLoseAnimation() {
		debugTextField.text = "Loser, ahahahah";
		minigame.SetActive(false);
		loseAnimation.SetActive(true);
		LeanTween.delayedCall(2.5f, () => { 
			base.ShowLoseAnimation();
		});
	}

	protected override void ShowWinAnimation() {
		debugTextField.text = "You win";
		winAnimation.SetActive(true);
		LeanTween.delayedCall(2.0f, () => {
			base.ShowWinAnimation();
		});
	}
}
