using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiShipPartsController : MonoBehaviour
{
    [Header("Context/Refs")]
    public SelectedShipContext selectedShipContext;

    [Tooltip("함선 선택 리스트 컨테이너(보이기/숨기기)")]
    public GameObject shipListContainer;

    [Tooltip("부품 리스트 컨테이너(보이기/숨기기)")]
    public GameObject partsListContainer;

    [Tooltip("부품 리스트 뷰 (선택 콜백 연결)")]
    public UiPartsList partsList;

    [Tooltip("설명 패널 (이번 작업에서는 아이콘 안 띄움)")]
    public UiPartExplain explain;

    [Header("기존 Address Text (이번엔 숨김 처리)")]
    public TextMeshProUGUI upAddressText;
    public TextMeshProUGUI leftAddressText;
    public TextMeshProUGUI rightAddressText;
    public TextMeshProUGUI downAddressText;

    [Header("아이콘 Image (아이콘만 보이게)")]
    public Image upIconImage;
    public Image leftIconImage;
    public Image rightIconImage;
    public Image downIconImage;

    [Header("아이콘 로드 설정")]
    [Tooltip("로드 실패 시 사용할 기본 아이콘 (선택)")]
    public Sprite defaultIcon;
    [Tooltip("Resources 기준 아이콘 경로 접두사 (확장자 제외)")]
    public string resourcesIconPrefix = "SciFiFlatSkills/Skills/";

    // ====== 틴트(색상) 설정 ======
    public enum TintMode { None, FixedPaletteByIconKey, FixedColor }

    [Header("아이콘 틴트(색상)")]
    [Tooltip("None: 무색, FixedPaletteByIconKey: 아이콘키 해시로 팔레트 선택, FixedColor: 모든 아이콘에 하나의 색")]
    public TintMode tintMode = TintMode.FixedPaletteByIconKey;

    [Tooltip("FixedColor 모드일 때 적용할 색")]
    public Color fixedTint = Color.white;

    [Tooltip("FixedPaletteByIconKey 모드에서 사용할 팔레트(비워두면 기본 4색 자동 세팅)")]
    public Color[] palette;

    private ShipSlot currentSlot = ShipSlot.Up;

    private void Awake()
    {
        // 팔레트 비어있으면 기본 4색 자동 구성(표준/희귀/영웅/전설 느낌)
        if (palette == null || palette.Length == 0)
        {
            palette = new Color[]
            {
                HexToColor("#FFFFFF"), // 일반(흰색)
                HexToColor("#7CFF00"), // 밝은 연두
                HexToColor("#00FFFD"), // 청록
                HexToColor("#006DFF"), // 파랑
            };
        }
    }

    private void Start()
    {
        // 기존 Address TMP는 숨김
        HideAddressTexts();

        // 패널 시작 모드
        SetModeShipList();

        // 아이콘 이미지 기본 설정(안전)
        SetupIconImage(upIconImage);
        SetupIconImage(leftIconImage);
        SetupIconImage(rightIconImage);
        SetupIconImage(downIconImage);

        // 초기값은 기본 아이콘 + 틴트 적용
        SetIconForSlot(ShipSlot.Up, null);
        SetIconForSlot(ShipSlot.Left, null);
        SetIconForSlot(ShipSlot.Right, null);
        SetIconForSlot(ShipSlot.Down, null);
    }

    private void HideAddressTexts()
    {
        if (upAddressText) upAddressText.gameObject.SetActive(false);
        if (leftAddressText) leftAddressText.gameObject.SetActive(false);
        if (rightAddressText) rightAddressText.gameObject.SetActive(false);
        if (downAddressText) downAddressText.gameObject.SetActive(false);
    }

    private void SetupIconImage(Image img)
    {
        if (!img) return;
        img.raycastTarget = false;
        img.preserveAspect = true;
        img.gameObject.SetActive(true);
    }

    // ====== 패널 버튼 연결 ======
    public void OnClickUpPanel() => OpenPartsForSlot(ShipSlot.Up);
    public void OnClickLeftPanel() => OpenPartsForSlot(ShipSlot.Left);
    public void OnClickRightPanel() => OpenPartsForSlot(ShipSlot.Right);
    public void OnClickDownPanel() => OpenPartsForSlot(ShipSlot.Down);

    private void OpenPartsForSlot(ShipSlot slot)
    {
        currentSlot = slot;
        SetModePartsList();

        if (!partsList)
        {
            Debug.LogError("[UiShipPartsController] partsList가 연결되어 있지 않습니다.");
            return;
        }

        partsList.Show(currentSlot, part =>
        {
            // 선택 반영
            OnSelectPart(part);

            // ID 저장(기존 방식 유지)
            if (selectedShipContext != null && selectedShipContext.ActiveShipId.HasValue)
            {
                selectedShipContext.SetPart(selectedShipContext.ActiveShipId.Value, currentSlot, part?.Id);
            }
        });

        // 설명 패널은 이번 작업 범위 외 (아이콘 표시 X)
        explain?.Clear();
    }

    private void OnSelectPart(PartData part)
    {
        if (part != null)
        {
            // 설명 텍스트 갱신 정도만(아이콘은 표시하지 않음)
            explain?.SetData(part);
        }

        // 아이콘 + 틴트 갱신
        SetIconForSlot(currentSlot, part);
    }

    public void ApplyPartToSlot(ShipSlot slot, PartData part) // 외부에서 직접 지정 시
    {
        if (part != null)
            explain?.SetData(part); // 설명은 유지(아이콘은 X)

        // 아이콘 + 틴트 갱신
        SetIconForSlot(slot, part);
    }

    public void ApplyPartsByIds(Dictionary<ShipSlot, int?> parts)
    {
        var partTable = DataTableManager.PartTable;
        if (parts == null || partTable == null) return;

        foreach (var kv in parts)
        {
            var id = kv.Value;
            PartData p = (id.HasValue) ? partTable.Get(id.Value) : null;
            ApplyPartToSlot(kv.Key, p);
        }
    }

    public void SetModeShipList()
    {
        if (shipListContainer) shipListContainer.SetActive(true);
        if (partsListContainer) partsListContainer.SetActive(false);
        explain?.Clear();
    }

    private void SetModePartsList()
    {
        if (shipListContainer) shipListContainer.SetActive(false);
        if (partsListContainer) partsListContainer.SetActive(true);
    }

    private Image GetIconImage(ShipSlot slot)
    {
        return slot switch
        {
            ShipSlot.Up => upIconImage,
            ShipSlot.Left => leftIconImage,
            ShipSlot.Right => rightIconImage,
            ShipSlot.Down => downIconImage,
            _ => null
        };
    }

    private void SetIconForSlot(ShipSlot slot, PartData part)
    {
        var targetImage = GetIconImage(slot);
        if (!targetImage) return;

        // 아이콘키 파싱
        string iconKey = AddressIconUtil.ExtractIconKey(part?.ItemAddress);

        // 로드(캐시 포함)
        Sprite s = IconSpriteCache.LoadSprite(resourcesIconPrefix, iconKey);

        // 실패 시 기본 아이콘
        if (!s && defaultIcon) s = defaultIcon;

        targetImage.sprite = s;
        targetImage.gameObject.SetActive(s != null);

        // === 틴트 적용 ===
        targetImage.color = ResolveTint(iconKey);
    }

    // ---------- Tint helpers ----------

    private Color ResolveTint(string iconKey)
    {
        switch (tintMode)
        {
            case TintMode.FixedColor:
                return fixedTint;

            case TintMode.FixedPaletteByIconKey:
                return PickFromPalette(iconKey);

            case TintMode.None:
            default:
                return Color.white;
        }
    }

    private Color PickFromPalette(string key)
    {
        if (palette == null || palette.Length == 0) return Color.white;

        uint h = StableHash(key ?? "");
        int idx = (int)(h % (uint)palette.Length);
        return palette[idx];
    }

    // FNV-1a 32bit: 런타임/플랫폼 간에도 안정적인 해시
    private static uint StableHash(string s)
    {
        const uint offset = 2166136261;
        const uint prime = 16777619;

        uint hash = offset;
        for (int i = 0; i < s.Length; i++)
        {
            hash ^= s[i];
            hash *= prime;
        }
        return hash;
    }

    private static Color HexToColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return Color.white;
        if (!hex.StartsWith("#")) hex = "#" + hex;
        if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
        return Color.white;
    }
}
