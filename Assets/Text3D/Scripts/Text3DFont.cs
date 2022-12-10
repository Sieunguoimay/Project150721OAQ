using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Text3D.Scripts
{
#if UNITY_EDITOR

    [System.Serializable]
    public enum Text3DJoin
    {
        Bevel = 0,
        Round = 1,
        Miter = 2
    }


    [System.Serializable]
    public enum Text3DCharacterRangeType
    {
        ControlCharacter,
        BasicLatin,
        ZeroToNine,
        Latin1Supplement,
        LatinExtendedA,
        LatinExtendedB,
        // IPAExtensions,
        // SpacingModifierLetters,
        // CombiningDiacriticalMarks,
        // GreekAndCoptic,
        // Cyrillic,
        // CyrillicSupplementary,
        // Armenian,
        // Hebrew,
        // Arabic,
        // Syriac,
        // ArabicSupplement,
        // Thaana,
        // NKo,
        // Samaritan,
        // Mandaic,
        // ArabicExtendedA,
        // Devanagari,
        // Bengali,
        // Gurmukhi,
        // Gujarati,
        // Oriya,
        // Tamil,
        // Telugu,
        // Kannada,
        // Malayalam,
        // Sinhala,
        // Thai,
        // Lao,
        // Tibetan,
        // Myanmar,
        // Georgian,
        // HangulJamo,
        // Ethiopic,
        // EthiopicSupplement,
        // Cherokee,
        // UnifiedCanadianAboriginalSyllabics,
        // Ogham,
        // Runic,
        // Tagalog,
        // Hanunoo,
        // Buhid,
        // Tagbanwa,
        // Khmer,
        // Mongolian,
        // UnifiedCanadianAboriginalSyllabicsExtended,
        // Limbu,
        // TaiLe,
        // NewTaiLue,
        // KhmerSymbols,
        // Buginese,
        // TaiTham,
        // CombiningDiacriticalMarksExtended,
        // Balinese,
        // Sundanese,
        // Batak,
        // Lepcha,
        // OlChiki,
        // CyrillicExtendedC,
        // SundaneseSupplement,
        // VedicExtensions,
        // PhoneticExtensions,
        // PhoneticExtensionsSupplement,
        // CombiningDiacriticalMarksSupplement,
        // LatinExtendedAdditional,
        // GreekExtended,
        // GeneralPunctuation,
        // SuperscriptsAndSubscripts,
        // CurrencySymbols,
        // CombiningDiacriticalMarksForSymbols,
        // LetterlikeSymbols,
        // NumberForms,
        // Arrows,
        // MathematicalOperators,
        // MiscellaneousTechnical,
        // ControlPictures,
        // OpticalCharacterRecognition,
        // EnclosedAlphanumerics,
        // BoxDrawing,
        // BlockElements,
        // GeometricShapes,
        // MiscellaneousSymbols,
        // Dingbats,
        // MiscellaneousMathematicalSymbolsA,
        // SupplementalArrowsA,
        // BraillePatterns,
        // SupplementalArrowsB,
        // MiscellaneousMathematicalSymbolsB,
        // SupplementalMathematicalOperators,
        // MiscellaneousSymbolsAndArrows,
        // Glagolitic,
        // LatinExtendedC,
        // Coptic,
        // GeorgianSupplement,
        // Tifinagh,
        // EthiopicExtended,
        // CyrillicExtendedA,
        // SupplementalPunctuation,
        // CJKRadicalsSupplement,
        // KangxiRadicals,
        // IdeographicDescriptionCharacters,
        // CJKSymbolsAndPunctuation,
        // Hiragana,
        // Katakana,
        // Bopomofo,
        // HangulCompatibilityJamo,
        // Kanbun,
        // BopomofoExtended,
        // CJKStrokes,
        // KatakanaPhoneticExtensions,
        // EnclosedCJKLettersAndMonths,
        // CJKCompatibility,
        // CJKUnifiedIdeographsExtensionA,
        // YijingHexagramSymbols,
        // CJKUnifiedIdeographs,
        // YiSyllables,
        // YiRadicals,
        // Lisu,
        // Vai,
        // CyrillicExtendedB,
        // Bamum,
        // ModifierToneLetters,
        // LatinExtendedD,
        // SylotiNagri,
        // CommonIndicNumberForms,
        // PhagsPa,
        // Saurashtra,
        // DevanagariExtended,
        // KayahLi,
        // Rejang,
        // HangulJamoExtendedA,
        // Javanese,
        // MyanmarExtendedB,
        // Cham,
        // MyanmarExtendedA,
        // TaiViet,
        // MeeteiMayekExtensions,
        // EthiopicExtendedA,
        // LatinExtendedE,
        // CherokeeSupplement,
        // MeeteiMayek,
        // HangulSyllables,
        // HangulJamoExtendedB,
        // HighSurrogates,
        // HighPrivateUseSurrogates,
        // LowSurrogates,
        // PrivateUseArea,
        // CJKCompatibilityIdeographs,
        // AlphabeticPresentationForms,
        // ArabicPresentationFormsA,
        // VariationSelectors,
        // VerticalForms,
        // CombiningHalfMarks,
        // CJKCompatibilityForms,
        // SmallFormVariants,
        // ArabicPresentationFormsB,
        // HalfwidthAndFullwidthForms,
        // Specials,
        // LinearBSyllabary,
        // LinearBIdeograms,
        // AegeanNumbers,
        // AncientGreekNumbers,
        // AncientSymbols,
        // PhaistosDisc,
        // Lycian,
        // Carian,
        // CopticEpactNumbers,
        // OldItalic,
        // Gothic,
        // OldPermic,
        // Ugaritic,
        // OldPersian,
        // Deseret,
        // Shavian,
        // Osmanya,
        // Osage,
        // Elbasan,
        // CaucasianAlbanian,
        // LinearA,
        // CypriotSyllabary,
        // ImperialAramaic,
        // Palmyrene,
        // Nabataean,
        // Hatran,
        // Phoenician,
        // Lydian,
        // MeroiticHieroglyphs,
        // MeroiticCursive,
        // Kharoshthi,
        // OldSouthArabian,
        // OldNorthArabian,
        // Manichaean,
        // Avestan,
        // InscriptionalParthian,
        // InscriptionalPahlavi,
        // PsalterPahlavi,
        // OldTurkic,
        // OldHungarian,
        // RumiNumeralSymbols,
        // Brahmi,
        // Kaithi,
        // SoraSompeng,
        // Chakma,
        // Mahajani,
        // Sharada,
        // SinhalaArchaicNumbers,
        // Khojki,
        // Multani,
        // Khudawadi,
        // Grantha,
        // Newa,
        // Tirhuta,
        // Siddham,
        // Modi,
        // MongolianSupplement,
        // Takri,
        // Ahom,
        // WarangCiti,
        // PauCinHau,
        // Bhaiksuki,
        // Marchen,
        // Cuneiform,
        // CuneiformNumbersAndPunctuation,
        // EarlyDynasticCuneiform,
        // EgyptianHieroglyphs,
        // AnatolianHieroglyphs,
        // BamumSupplement,
        // Mro,
        // BassaVah,
        // PahawhHmong,
        // Miao,
        // IdeographicSymbolsAndPunctuation,
        // Tangut,
        // TangutComponents,
        // KanaSupplement,
        // Duployan,
        // ShorthandFormatControls,
        // ByzantineMusicalSymbols,
        // MusicalSymbols,
        // AncientGreekMusicalNotation,
        // TaiXuanJingSymbols,
        // CountingRodNumerals,
        // MathematicalAlphanumericSymbols,
        // SuttonSignWriting,
        // GlagoliticSupplement,
        // MendeKikakui,
        // Adlam,
        // ArabicMathematicalAlphabeticSymbols,
        // MahjongTiles,
        // DominoTiles,
        // PlayingCards,
        // EnclosedAlphanumericSupplement,
        // EnclosedIdeographicSupplement,
        // MiscellaneousSymbolsAndPictographs,
        // EmoticonsEmoji,
        // OrnamentalDingbats,
        // TransportAndMapSymbols,
        // AlchemicalSymbols,
        // GeometricShapesExtended,
        // SupplementalArrowsC,
        // SupplementalSymbolsAndPictographs,
        // CJKUnifiedIdeographsExtensionB,
        // CJKUnifiedIdeographsExtensionC,
        // CJKUnifiedIdeographsExtensionD,
        // CJKUnifiedIdeographsExtensionE,
        // CJKCompatibilityIdeographsSupplement,
        // Tags,
        // VariationSelectorsSupplement,
        Custom
    }


    [System.Serializable]
    public struct Text3DCharacterRange
    {
        public string custom;
        public char begin;
        public char end;
        public Text3DCharacterRangeType type;
        public Text3DCharacterRange(int b, int e, Text3DCharacterRangeType t)
        {
            custom = "";
            begin = (char) b;
            end = (char) e;
            type = t;
        }
    }

#endif


    [System.Serializable]
    public struct Text3DKerningPair
    {
        public Vector2 value;
        public char left;
        public char right;

        public Text3DKerningPair(char l, char r, Vector2 v)
        {
            value = v;
            left = l;
            right = r;
        }
    }


    [System.Serializable]
    public class Text3DGlyph
    {
        public Mesh mesh = null;
        public Vector2 advance = Vector2.zero;
        public int id = 0;
    }


    public class Text3DFont : ScriptableObject
    {
        [SerializeField] private List<Text3DGlyph> glyphList = null;
        [SerializeField] private List<Text3DKerningPair> kerningPairList = null;
        [SerializeField] private Vector2 wordSpace = Vector2.zero;
        [SerializeField] private Vector2 lineSpace = Vector2.zero;
        [SerializeField] private float extrude = 0.0f;
        [SerializeField] private float outlineWidth = 0.0f;
        [SerializeField] private int unitsPerEm = 1;
        [SerializeField] private int missingGlyph = 127;

#if UNITY_EDITOR

        [SerializeField] private List<Text3DCharacterRange> characterRangeList = new();
        [SerializeField] private ModelImporterMeshCompression meshCompression = ModelImporterMeshCompression.Off;
        [SerializeField] private Text3DJoin outlineJoin = Text3DJoin.Bevel;
        [SerializeField] private Color32 topLeftColor = Color.black;
        [SerializeField] private Color32 topRightColor = Color.black;
        [SerializeField] private Color32 bottomLeftColor = Color.black;
        [SerializeField] private Color32 bottomRightColor = Color.black;
        [SerializeField] private string assetPath = "";
        [SerializeField] private string fontName = "";
        [SerializeField] private float curveQuality = 1.0f;
        [SerializeField] private float miterLimit = 4.0f;
        [SerializeField] private int bevelSegments = 0;
        [SerializeField] private bool optimizeMesh = false;
        [SerializeField] private bool useTangents = false;
        [SerializeField] private bool useColors = false;
        [SerializeField] private bool uvCorrection = false;

        private Text3DGlyph[] _previewGlyphs;
        private string _infoString = "";
        private readonly List<Text3DCharacterRange> _oldCharacterRangeList = new();
        private ModelImporterMeshCompression _oldMeshCompression;
        private Text3DJoin _oldOutlineJoin;
        private Color32 _oldTopLeftColor;
        private Color32 _oldTopRightColor;
        private Color32 _oldBottomLeftColor;
        private Color32 _oldBottomRightColor;
        private string _oldAssetPath;
        private string _oldFontName;
        private float _oldCurveQuality;
        private float _oldMiterLimit;
        private float _oldExtrude;
        private float _oldOutlineWidth;
        private int _oldBevelSegments;
        private int _oldMissingGlyph;
        private bool _oldOptimizeMesh;
        private bool _oldUseTangents;
        private bool _oldUseColors;
        private bool _oldUvCorrection;
        private bool _snapShot;
#endif

        private Dictionary<int, Text3DGlyph> _glyphMap;
        private Dictionary<uint, Vector2> _kerningPairMap;

        public Text3DFont()
        {
            _kerningPairMap = null;
        }

        public int UnitsPerEm => unitsPerEm;
        public Vector2 WordSpace => wordSpace;
        public Vector2 LineSpace => lineSpace;


#if UNITY_EDITOR

        public delegate bool LoadFontDelegate(string file, ref int units, ref Vector2 lineSpace, ref Vector2 wordSpace, ref string fontName);

        public delegate void LoadKerningPairsDelegate(Dictionary<uint, Vector2> map, List<Text3DKerningPair> list);

        public delegate void CreateGlyphDelegate(Text3DFont asset, char c, Material face, Material side, Material outline, Color32 tl, Color32 tr, Color32 bl, Color32 br, Text3DJoin join, float limit, float width, float extrude, float quality, int segments, bool tangents, bool colors, bool correction);

        public Text3DGlyph[] PreviewGlyphs
        {
            get
            {
                if (null != _previewGlyphs || string.IsNullOrEmpty(fontName)) return _previewGlyphs;
                var pos = Vector3.zero;
                var offset = Vector3.zero;
                var left = float.MaxValue;
                var top = float.MinValue;
                var right = float.MinValue;
                var bottom = float.MaxValue;
                var count = 0;

                _previewGlyphs = new Text3DGlyph[fontName.Length];
                PreviewPositions = new Vector3[fontName.Length];

                for (var i = 0; i < fontName.Length; ++i)
                {
                    _previewGlyphs[i] = null;
                    PreviewPositions[i] = Vector3.zero;
                }

                for (var i = 0; i < fontName.Length; ++i)
                {
                    pos.x = 0.0f;
                    var begin = count;

                    while (i < fontName.Length && ' ' != fontName[i] && '\n' != fontName[i])
                    {
                        var curr = fontName[i++];
                        var next = (fontName.Length != i) ? fontName[i] : '\0';

                        _previewGlyphs[count] = GetGlyph(curr);
                        PreviewPositions[count] = pos;

                        if (null == _previewGlyphs[count]) continue;
                        top = ((_previewGlyphs[count].mesh.bounds.max.y + pos.y) > top) ? _previewGlyphs[count].mesh.bounds.max.y + pos.y : top;
                        bottom = ((_previewGlyphs[count].mesh.bounds.min.y + pos.y) < bottom) ? _previewGlyphs[count].mesh.bounds.min.y + pos.y : bottom;
                        pos.x += _previewGlyphs[count++].advance.x + GetKerning(curr, next, false);
                    }

                    if (count > 0 && begin != count)
                    {
                        var min = _previewGlyphs[begin].mesh.bounds.min;
                        var max = _previewGlyphs[count - 1].mesh.bounds.max;
                        offset.x = (max.x + PreviewPositions[count - 1].x - min.x - PreviewPositions[begin].x) * 0.5f;

                        for (var j = begin; j < count; ++j)
                            PreviewPositions[j].x -= offset.x;

                        left = ((min.x + PreviewPositions[begin].x) < left) ? min.x + PreviewPositions[begin].x : left;
                        right = ((max.x + PreviewPositions[count - 1].x) > right) ? max.x + PreviewPositions[count - 1].x : right;
                    }

                    pos.y -= lineSpace.x;
                }

                offset.Set(0.0f, (top + bottom) * 0.5f, (((right - left) * 0.5f) / 0.26794919243112270647255365849413f) * 1.15f + extrude);

                for (var i = 0; i < count; ++i)
                    PreviewPositions[i] -= offset;

                return _previewGlyphs;
            }
        }

        [field: System.NonSerialized] public Vector3[] PreviewPositions { get; private set; }

        public string InfoString
        {
            get
            {
                if (glyphList is not {Count: > 0} || !string.IsNullOrEmpty(_infoString)) return _infoString;
                var vertices = 0;
                var primitives = 0;

                foreach (var g in glyphList)
                {
                    vertices += g.mesh.vertexCount;
                    primitives += g.mesh.triangles.Length;
                }

                _infoString = fontName + " " + vertices + " verts, " + (primitives / 3) + " tris";

                if (glyphList[0].mesh.subMeshCount > 1)
                    _infoString += ", " + glyphList[0].mesh.subMeshCount + " subMeshes";

                _infoString += " uv";

                if (uvCorrection)
                    _infoString += ",uv2,uv3";

                if (useColors)
                    _infoString += ",colors";

                return _infoString;
            }
        }

        public IEnumerable<Text3DGlyph> GlyphList => glyphList;

        public void CreateSnapshot()
        {
            if (_snapShot)
                return;

            _oldCharacterRangeList.Clear();
            _oldCharacterRangeList.AddRange(characterRangeList);

            _oldMeshCompression = meshCompression;
            _oldOutlineJoin = outlineJoin;
            // oldFaceMaterial     = faceMaterial;
            // oldSideMaterial     = sideMaterial;
            // oldOutlineMaterial  = outlineMaterial;
            _oldTopLeftColor = topLeftColor;
            _oldTopRightColor = topRightColor;
            _oldBottomLeftColor = bottomLeftColor;
            _oldBottomRightColor = bottomRightColor;
            _oldAssetPath = assetPath;
            _oldFontName = fontName;
            _oldCurveQuality = curveQuality;
            _oldMiterLimit = miterLimit;
            _oldExtrude = extrude;
            _oldOutlineWidth = outlineWidth;
            _oldBevelSegments = bevelSegments;
            _oldMissingGlyph = missingGlyph;
            _oldOptimizeMesh = optimizeMesh;
            _oldUseTangents = useTangents;
            _oldUseColors = useColors;
            _oldUvCorrection = uvCorrection;
            _snapShot = true;
        }

        public void Revert()
        {
            characterRangeList.Clear();
            characterRangeList.AddRange(_oldCharacterRangeList);

            meshCompression = _oldMeshCompression;
            outlineJoin = _oldOutlineJoin;
            // faceMaterial     = oldFaceMaterial;
            // sideMaterial     = oldSideMaterial;
            // outlineMaterial  = oldOutlineMaterial;
            topLeftColor = _oldTopLeftColor;
            topRightColor = _oldTopRightColor;
            bottomLeftColor = _oldBottomLeftColor;
            bottomRightColor = _oldBottomRightColor;
            assetPath = _oldAssetPath;
            fontName = _oldFontName;
            curveQuality = _oldCurveQuality;
            miterLimit = _oldMiterLimit;
            extrude = _oldExtrude;
            outlineWidth = _oldOutlineWidth;
            bevelSegments = _oldBevelSegments;
            missingGlyph = _oldMissingGlyph;
            optimizeMesh = _oldOptimizeMesh;
            useTangents = _oldUseTangents;
            useColors = _oldUseColors;
            uvCorrection = _oldUvCorrection;
        }

        public void Apply(LoadFontDelegate load, LoadKerningPairsDelegate kerning, CreateGlyphDelegate create)
        {
            var actors = Object.FindObjectsOfType<Text3D>();
            var list = new List<Text3D>();
            var found = true;

            PreviewPositions = null;
            _previewGlyphs = null;
            fontName = "";
            _infoString = "";
            _snapShot = false;

            _glyphMap ??= new Dictionary<int, Text3DGlyph>();
            _kerningPairMap ??= new Dictionary<uint, Vector2>();
            glyphList ??= new List<Text3DGlyph>();
            kerningPairList ??= new List<Text3DKerningPair>();

            _glyphMap.Clear();
            _kerningPairMap.Clear();
            glyphList.Clear();
            kerningPairList.Clear();

            foreach (var a in actors)
            {
                if (a.SourceFont != this) continue;
                a.SetFont(null, true);
                list.Add(a);
            }

            while (found)
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
                found = false;

                foreach (var a in assets)
                {
                    if (a is not Mesh) continue;
                    DestroyImmediate(a, true);
                    found = true;
                }
            }

            if (!load(assetPath, ref unitsPerEm, ref lineSpace, ref wordSpace, ref fontName))
                return;

            foreach (var r in characterRangeList)
            {
                if (string.IsNullOrEmpty(r.custom))
                {
                    for (var i = r.begin; i <= r.end; ++i)
                    {
                        if (!_glyphMap.ContainsKey(i))
                            create(this, i, null, null, null, topLeftColor, topRightColor, bottomLeftColor, bottomRightColor, outlineJoin, miterLimit, outlineWidth, extrude, curveQuality, bevelSegments, useTangents, useColors, uvCorrection);
                    }
                }
                else
                {
                    foreach (var t in r.custom.Where(t => !_glyphMap.ContainsKey(t)))
                    {
                        create(this, t, null, null, null, topLeftColor, topRightColor, bottomLeftColor, bottomRightColor, outlineJoin, miterLimit, outlineWidth, extrude, curveQuality, bevelSegments, useTangents, useColors, uvCorrection);
                    }
                }
            }

            if (!_glyphMap.ContainsKey((char) missingGlyph))
                create(this, (char) missingGlyph, null, null, null, topLeftColor, topRightColor, bottomLeftColor, bottomRightColor, outlineJoin, miterLimit, outlineWidth, extrude, curveQuality, bevelSegments, useTangents, useColors, uvCorrection);

            kerning(_kerningPairMap, kerningPairList);

            foreach (var a in list)
                a.SetFont(this, true);

            CreateSnapshot();
            list.Clear();
            EditorUtility.SetDirty(this);
        }

        public void AddGlyph(Text3DGlyph glyph)
        {
            _glyphMap.Add(glyph.id, glyph);
            glyphList.Add(glyph);

            if (optimizeMesh)
                MeshUtility.Optimize(glyph.mesh);

            MeshUtility.SetMeshCompression(glyph.mesh, meshCompression);
            AssetDatabase.AddObjectToAsset(glyph.mesh, this);
        }

#endif

        private void LoadFontData()
        {
            _glyphMap = new Dictionary<int, Text3DGlyph>();
            _kerningPairMap = new Dictionary<uint, Vector2>();

            foreach (var g in glyphList.Where(g => !_glyphMap.ContainsKey(g.id)))
            {
                _glyphMap.Add(g.id, g);
            }

            foreach (var p in kerningPairList)
            {
                var key = (uint) (p.right << 16) | p.left;

                if (!_kerningPairMap.ContainsKey(key))
                    _kerningPairMap.Add(key, p.value);
            }
        }

        public Text3DGlyph GetGlyph(char c)
        {
            if (null == _glyphMap)
                LoadFontData();

            if (_glyphMap.ContainsKey(c))
                return _glyphMap[c];
            return _glyphMap.ContainsKey(missingGlyph) ? _glyphMap[missingGlyph] : null;
        }

        public float GetKerning(char left, char right, bool vertical)
        {
            var key = (uint) (right << 16) | left;

            if (null == _kerningPairMap)
                LoadFontData();

            if (_kerningPairMap.ContainsKey(key))
                return vertical ? _kerningPairMap[key].y : _kerningPairMap[key].x;

            return 0.0f;
        }
    }
}