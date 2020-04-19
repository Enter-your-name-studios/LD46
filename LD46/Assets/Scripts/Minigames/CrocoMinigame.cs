﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CrocoMinigame : BaseMinigame {
  public Transform Crocodile;
  public Transform Toothbrush;

	public void Win() {
		isPlaying = false;
		ShowWinAnimation();
	}

	protected override void ShowLoseAnimation() {
		LeanTween.delayedCall(1.0f, () => {
			base.ShowLoseAnimation();
		});
	}

	protected override void ShowWinAnimation() {
		LeanTween.delayedCall(1.0f, () => {
			base.ShowWinAnimation();
		});
	}
}
