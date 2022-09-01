using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;
using UnityEditor;

public class Manager : MonoBehaviour
{
    [SerializeField] Slider moodBar;

    [SerializeField] Text dayIncomeText, dayNumberText, netWorthText, costsText, workersText, dayProfitText, gameOverText, rentText, wageText, finalDayText,
        dayhighScoreText, peakWorthText, worthHighScoreText;

    [SerializeField] GameObject gameOverScreen, worker, instruction, rentIncrease;

    [SerializeField] Sprite buttonUp, buttonDown;

    [SerializeField] SpriteRenderer overlay, button;

    [SerializeField] AudioClip moneyUp, moneyDown, gameEnd;

    [SerializeField] AudioClip[] bells, offices, talking, parties;

    [SerializeField] ParticleSystem[] steams;

    [SerializeField] PostProcessProfile main;

    ColorGrading colourGrade;

    AudioSource moneyAudio, humAudio, hissAudio, quitAudio, eventAudio, lowAmb, midAmb, highAmb;

    List<Worker> workerScripts = new List<Worker>();

    bool isPlaying, onMenu, gameEnded, workersLeaving, counting;

    float moodLevel;

    int workers, addedWorkers, dayNumber, expectedIncome, costs, expectedProfit, netWorth, rent, wages, saturationState, peakNetworth;

    Vector3 leftSpawn = new Vector3(-4.7f, 4.8f, 0), rightSpawn = new Vector3(4.7f, 4.8f, 0);

    private void Awake()
    {
        colourGrade = main.GetSetting<ColorGrading>();
        AudioSource[] sources = GetComponents<AudioSource>();

        moneyAudio = sources[0];
        humAudio = sources[1];
        hissAudio = sources[2];
        quitAudio = sources[3];
        eventAudio = sources[4];
        lowAmb = sources[5];
        midAmb = sources[6];
        highAmb = sources[7];
    }

    private void Start()
    {
        ResetGame();
        StartCoroutine(DayCycle());
    }

    void Update()
    {
        if (isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                button.sprite = buttonDown;

                humAudio.volume = 1;

                hissAudio.Play();
                humAudio.Play();

                foreach(ParticleSystem steam in steams)
                {
                    steam.Play();
                }
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                button.sprite = buttonUp;

                StartCoroutine(AudioFade(humAudio));

                foreach (ParticleSystem steam in steams)
                {
                    steam.Stop();
                }
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                moodLevel += 4f * Time.deltaTime;

                if (!humAudio.isPlaying)
                {
                    humAudio.Play();
                }
            }
            else
            {
                moodLevel -= 2f * Time.deltaTime;
            }

            moodLevel = Mathf.Clamp(moodLevel, 10f, 100f);
            moodBar.value = moodLevel;

            float adjustedMood = moodLevel - 10f;
            lowAmb.volume = Mathf.Clamp(((90 - adjustedMood) / 45) - 1, 0f, 1f);
            midAmb.volume = Mathf.Clamp(1 - (Mathf.Abs(45 - adjustedMood) / 15), 0f, 1f);
            highAmb.volume = Mathf.Clamp(1 - ((90 - adjustedMood) / 45), 0f, 1f);

            expectedIncome = Mathf.RoundToInt((workers * 1500 / moodLevel));
            expectedIncome = Mathf.Clamp(expectedIncome, 0, 10000);

            expectedProfit = expectedIncome - costs;

            addedWorkers = Mathf.RoundToInt(3 - ((90 - moodLevel) / 15));
            addedWorkers = Mathf.Clamp(addedWorkers, 0, 3);

            if(addedWorkers == 3 && saturationState != 2)
            {
                StartCoroutine(SaturationFade(100));
                saturationState = 2;
            }
            else if(moodLevel > 30.1f && saturationState != 1)
            {
                StartCoroutine(SaturationFade(0));
                saturationState = 1;
            }
            else if(moodLevel < 30)
            {
                if(saturationState != 0)
                {
                    StartCoroutine(SaturationFade(-75));
                    saturationState = 0;
                    StartCoroutine(WorkersLeaving());
                }
                else if (!workersLeaving)
                {
                    StartCoroutine(WorkersLeaving());
                }
            }

            dayIncomeText.text = "Income: +£" + expectedIncome;
            dayProfitText.text = "Profit: £" + expectedProfit;
            workersText.text = "Workers: " + workers + " + " + addedWorkers;

            
        }
        else if(onMenu)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if(!gameEnded)
                {
                    instruction.SetActive(false);

                    dayNumber++;
                    dayNumberText.text = dayNumber.ToString();

                    if(dayNumber % 3 == 0)
                    {
                        StartCoroutine(PopUp(rentIncrease, 1f));
                        rent += 50;
                        wages += 2;
                        rentText.text = "Rent: £" + rent;
                        wageText.text = "Wages: £" + wages;
                    }

                    costs = rent + (workers * wages);
                    costsText.text = "Costs: -£" + costs;

                    SpawnWorkers(addedWorkers);

                    onMenu = false;
                    isPlaying = true;

                    StartCoroutine(DayCycle());
                    
                }
                else
                {
                    ResetGame();

                    StartCoroutine(DayCycle());
                }
            }
        }
    }

    private void ResetGame()
    {
        foreach (Worker workerScript in workerScripts)
        {
            Destroy(workerScript.gameObject);
        }

        workerScripts.Clear();

        colourGrade.saturation.value = 0;

        saturationState = 1;
        netWorth = 100;
        peakNetworth = 100;
        moodLevel = 50;
        workers = 10;
        dayNumber = 1;
        rent = 100;
        wages = 10;
        costs = rent + (workers * wages);

        expectedProfit = expectedIncome - costs;

        lowAmb.volume = 0;
        midAmb.volume = 1;
        highAmb.volume = 0;
        isPlaying = true;
        onMenu = false;
        gameEnded = false;
        workersLeaving = false;

        dayNumberText.text = "1";
        dayIncomeText.text = "Income: +£0";
        costsText.text = "Costs: -£" + costs;
        dayProfitText.text = "Profit: £" + expectedProfit;
        workersText.text = "Workers: " + workers;
        netWorthText.text = "Net Worth: £" + netWorth;
        rentText.text = "Rent: £" + rent;
        wageText.text = "Wages: £" + wages;

        gameOverScreen.SetActive(false);
        instruction.SetActive(false);
        rentIncrease.SetActive(false);

        SpawnWorkers(workers);
    }

    private void GameOver(string reason)
    {
        eventAudio.clip = gameEnd;
        eventAudio.Play();

        gameOverScreen.SetActive(true);
        gameOverText.text = reason;
        finalDayText.text = "Days Running: " + dayNumber;
        peakWorthText.text = "Peak Net Worth: £" + peakNetworth;

        if(PlayerPrefs.HasKey("Days"))
        {
            int highScore = PlayerPrefs.GetInt("Days");

            if(highScore < dayNumber)
            {
                PlayerPrefs.SetInt("Days", dayNumber);
            }
        }
        else
        {
            PlayerPrefs.SetInt("Days", dayNumber);
        }

        if (PlayerPrefs.HasKey("Worth"))
        {
            int peak = PlayerPrefs.GetInt("Worth");

            if (peak < peakNetworth)
            {
                PlayerPrefs.SetInt("Worth", peakNetworth);
            }
        }
        else
        {
            PlayerPrefs.SetInt("Worth", peakNetworth);
        }

        dayhighScoreText.text = "Longest Running: " + PlayerPrefs.GetInt("Days");
        worthHighScoreText.text = "Best Net Worth: £" + PlayerPrefs.GetInt("Worth");

        gameEnded = true;
    }

    private IEnumerator DayCycle()
    {
        eventAudio.clip = bells[Random.Range(0, 7)];
        lowAmb.clip = offices[Random.Range(0, 5)];
        midAmb.clip = talking[Random.Range(0, 5)];
        highAmb.clip = parties[Random.Range(0, 5)];

        eventAudio.Play();
        lowAmb.Play();
        midAmb.Play();
        highAmb.Play();

        StartCoroutine(FadeOverlay(true));

        foreach (Worker worker in workerScripts)
        {
            worker.Arrive();
        }

        yield return new WaitForSeconds(15);

        eventAudio.clip = bells[Random.Range(0, 7)];
        eventAudio.Play();

        button.sprite = buttonUp;

        StartCoroutine(AudioFade(humAudio));

        foreach (ParticleSystem steam in steams)
        {
            steam.Stop();
        }

        foreach (Worker worker in workerScripts)
        {
            worker.Leave();
        }

        StartCoroutine(FadeOverlay(false));

        isPlaying = false;

        if (expectedIncome != 0)
        {
            StartCoroutine(NetWorthCountUp());
        }

        StartCoroutine(WorkersCountUp());

        yield return new WaitForSeconds(3f);

        counting = false;

        instruction.SetActive(true);

        onMenu = true;
    }

    private IEnumerator WorkersLeaving()
    {
        workersLeaving = true;

        while(moodLevel < 30 && isPlaying)
        {
            yield return new WaitForSeconds(4.5f);

            if (moodLevel < 30 && isPlaying)
            {
                workers -= 1;
                workersText.text = "Workers: " + workers;

                costs = rent + (workers * wages);
                costsText.text = "Costs: -£" + costs;

                quitAudio.Play();

                Worker quitter = workerScripts[Random.Range(0, workerScripts.Count)];
                quitter.quit = true;
                quitter.Leave();
                quitter.textPopUp.SetActive(true);
                workerScripts.Remove(quitter);
            }

            if(workers == 0)
            {
                GameOver("You have no one working for you!");
            }
        }

        workersLeaving = false;
    }

    private void SpawnWorkers(int number)
    {
        int side = Random.Range(0, 2);
        bool left = side == 0;

        for (int i = 0; i < number; i++)
        {
            GameObject obj = Instantiate(worker, left ? leftSpawn:rightSpawn, Quaternion.identity);

            Worker workerScript = obj.GetComponent<Worker>();
            workerScripts.Add(workerScript);
            workerScript.left = left;

            left = !left;
        }
    }

    private IEnumerator FadeOverlay(bool fadeIn)
    {
        overlay.DOFade(fadeIn ? 0 : 0.5f, 0.5f);

        yield return null;
    }

    private IEnumerator NetWorthCountUp()
    {
        counting = true;

        int worthTarget = netWorth + expectedProfit;

        int step;

        if(worthTarget - netWorth > 0)
        {
            step = 1;
            moneyAudio.clip = moneyUp;
        }
        else
        {
            step = -1;
            moneyAudio.clip = moneyDown;
        }

        float timeStep = 3 / expectedIncome;

        moneyAudio.Play();

        while(netWorth != worthTarget)
        {
            if(step == 1 && netWorth > worthTarget)
            {
                break;
            }
            else if(step == -1 && netWorth < worthTarget)
            {
                break;
            }
            else if (!counting)
            {
                netWorth = worthTarget;
                netWorthText.text = "Net Worth: £" + netWorth;
                break;
            }

            netWorth += step;
            netWorthText.text = "Net Worth: £" + netWorth;
            yield return new WaitForSeconds(timeStep);
        }

        counting = false;

        if (netWorth <= 0)
        {
            GameOver("You have no more money!");
        }
        else if(peakNetworth < netWorth)
        {
            peakNetworth = netWorth;
        }

        moneyAudio.Stop();
    }

    private IEnumerator WorkersCountUp()
    {
        int workerTarget = workers + addedWorkers;

        while (workers != workerTarget)
        {
            workers ++;
            workersText.text = "Workers: " + workers;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator PopUp(GameObject popUp, float duration)
    {
        popUp.SetActive(true);

        yield return new WaitForSeconds(duration);

        popUp.SetActive(false);
    }

    private IEnumerator AudioFade(AudioSource source)
    {
        source.DOFade(0, 0.2f);

        yield return null;
    }

    private IEnumerator SaturationFade(float targetSat)
    {
        float sat = colourGrade.saturation;

        var tweener = DOTween.To(() => sat, x => sat = x, targetSat, 1f)
            .SetEase(Ease.OutExpo)
            .OnUpdate(() => colourGrade.saturation.value = sat);

        while (tweener.IsActive())
        {
            yield return null;
        }
    }
}
