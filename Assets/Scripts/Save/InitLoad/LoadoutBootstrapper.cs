using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class LoadoutBootstrapper : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("현재 선택/장착 상태를 들고 있는 컨텍스트")]
    public SelectedShipContext selectedShipContext;

    [Tooltip("부품 UI 컨트롤러 (ApplyPartsByIds 사용)")]
    public UiShipPartsController partsUI;

    [Tooltip("무장 UI 컨트롤러 (ApplyWeaponsByIds 사용, 선택)")]
    public UiShipWeaponsController weaponsUI;

    [Header("Behavior")]
    [Tooltip("최초 실행에 한해 기본 JSON으로 파일을 생성/덮어쓰기(권장: 켜기)")]
    public bool initializeOnFirstLaunch = true;

    [Tooltip("앱을 켤 때마다 현재 저장 파일을 읽어 UI에 자동 적용(되돌리기 자동 실행)")]
    public bool autoApplyOnEveryLaunch = true;

    [Tooltip("DataTableManager 등이 준비될 때까지 기다렸다가 적용(권장)")]
    public bool waitForDataTables = true;

    [Tooltip("테이블 준비 대기 시간초과(초). 0이면 무제한 대기")]
    public float waitTimeoutSeconds = 5f;

    [Header("Defaults")]
    [Tooltip("최초 초기화에 사용할 템플릿 버전")]
    public int templateVersion = 2;

    [Tooltip("최초 활성화 shipId (템플릿에 존재하는 ID로 설정)")]
    public int defaultActiveShipId = 41;

    private const string PrefsKeyInitDone = "LOADOUT_INIT_DONE_v2";
    private static readonly JsonSerializerSettings JsonSettings = new() { Formatting = Formatting.Indented };
    private const string DefaultJson = @"
{
  ""version"": 2,
  ""ships"": {
    ""41"": {
      ""shipKey"": 41,
      ""parts"": {
        ""Up"": 1435000100,
        ""Left"": 1232000200,
        ""Right"": 1334000200,
        ""Down"": 1131000300
      },
      ""weapons"": {}
    }
  }
}";

    private void Awake()
    {
        if (initializeOnFirstLaunch && !PlayerPrefs.HasKey(PrefsKeyInitDone))
        {
            TryWriteDefaultJsonToFile(forceOverwrite: true);
            PlayerPrefs.SetInt(PrefsKeyInitDone, 1);
            PlayerPrefs.Save();
        }
    }

    private void Start()
    {
        if (autoApplyOnEveryLaunch)
        {
            StartCoroutine(CoApplyFromSaveWhenReady());
        }
    }

    private void TryWriteDefaultJsonToFile(bool forceOverwrite)
    {
        try
        {
            SavePath.EnsureDir();

            if (!forceOverwrite && File.Exists(SavePath.LoadoutJsonPath))
                return;

            var model = JsonConvert.DeserializeObject<LoadoutSaveModel>(DefaultJson) ?? new LoadoutSaveModel();
            model.Version = Mathf.Max(model.Version, templateVersion);

            var json = JsonConvert.SerializeObject(model, JsonSettings);
            File.WriteAllText(SavePath.LoadoutJsonPath, json);

            Debug.Log($"[LoadoutBootstrapper] 초기 템플릿 저장 완료: {SavePath.LoadoutJsonPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LoadoutBootstrapper] 초기 템플릿 저장 실패: {ex.Message}");
        }
    }

    private IEnumerator CoApplyFromSaveWhenReady()
    {
        if (waitForDataTables)
        {
            float t = 0f;
            while (!IsDataTablesReady())
            {
                if (waitTimeoutSeconds > 0f && t > waitTimeoutSeconds)
                {
                    Debug.LogWarning("[LoadoutBootstrapper] DataTable 준비 대기 시간초과. 그래도 적용 시도.");
                    break;
                }
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        var model = TryLoadModelFromFile();
        if (model == null || model.Ships == null || model.Ships.Count == 0)
        {
            Debug.LogWarning("[LoadoutBootstrapper] 세이브가 비어있음. 템플릿으로 재생성 후 재시도.");
            TryWriteDefaultJsonToFile(forceOverwrite: true);
            model = TryLoadModelFromFile();
            if (model == null)
            {
                Debug.LogError("[LoadoutBootstrapper] 템플릿 재생성 후에도 로드 실패. 자동 적용 중단.");
                yield break;
            }
        }

        if (selectedShipContext == null)
        {
            Debug.LogError("[LoadoutBootstrapper] SelectedShipContext 미할당. 자동 적용 불가.");
            yield break;
        }

        selectedShipContext.ClearAll();

        int activeId = defaultActiveShipId;
        if (!model.Ships.ContainsKey(activeId))
        {
            foreach (var kv in model.Ships)
            {
                activeId = kv.Key;
                break;
            }
        }
        selectedShipContext.SetActiveShip(activeId);

        foreach (var pair in model.Ships)
        {
            int shipId = pair.Key;
            var entry = pair.Value;

            if (entry?.Parts != null)
            {
                foreach (var kv in entry.Parts)
                    selectedShipContext.SetPart(shipId, kv.Key, kv.Value);
            }

            if (entry?.Weapons != null)
            {
                foreach (var kv in entry.Weapons)
                    selectedShipContext.SetWeapon(shipId, kv.Key, kv.Value);
            }
        }

        if (model.Ships.TryGetValue(selectedShipContext.ActiveShipId ?? activeId, out var activeLoadout))
        {
            if (partsUI != null)
                partsUI.ApplyPartsByIds(activeLoadout.Parts);

            if (weaponsUI != null)
                weaponsUI.ApplyWeaponsByIds(activeLoadout.Weapons);
        }
        else
        {
            Debug.LogWarning("[LoadoutBootstrapper] ActiveShip 데이터가 모델에 없음. UI 반영 생략.");
        }

        Debug.Log("[LoadoutBootstrapper] 자동 적용(되돌리기 자동 실행) 완료.");
    }

    private static LoadoutSaveModel TryLoadModelFromFile()
    {
        try
        {
            if (!File.Exists(SavePath.LoadoutJsonPath))
                return null;

            var json = File.ReadAllText(SavePath.LoadoutJsonPath);
            return JsonConvert.DeserializeObject<LoadoutSaveModel>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LoadoutBootstrapper] 저장 파일 로드 실패: {ex.Message}");
            return null;
        }
    }

    private static bool IsDataTablesReady()
    {
        return DataTableManager.ShipTable != null;
    }

#if UNITY_EDITOR
    [ContextMenu("Dev/Reset First Launch Flag")]
    private void DevResetFlag()
    {
        PlayerPrefs.DeleteKey(PrefsKeyInitDone);
        PlayerPrefs.Save();
        Debug.Log("[LoadoutBootstrapper] First Launch Flag 초기화 완료.");
    }

    [ContextMenu("Dev/Force Write Default JSON")]
    private void DevForceWrite()
    {
        TryWriteDefaultJsonToFile(forceOverwrite: true);
        Debug.Log("[LoadoutBootstrapper] 템플릿 강제 저장 완료.");
    }
#endif
}
