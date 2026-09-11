# 공유 PBR 텍스처

2026-09-11 Poly Haven 공식 배포본에서 내려받은 무료 CC0 에셋입니다. Unity Web에는 적용·빌드했으며 Unreal은 동일 원본을 가져올 수 있도록 DirectX 노멀도 보관했습니다. Unreal 에디터 적용은 아직 수행하지 않았습니다.

| 에셋 | 제작자 | 사용 위치 |
|---|---|---|
| [asphalt_02](https://polyhaven.com/a/asphalt_02) | Rob Tuytel | Unity 바닥 |
| [concrete_wall_003](https://polyhaven.com/a/concrete_wall_003) | Dimitrios Savva (촬영), Rico Cilliers (처리) | Unity 벽·건물·창틀 |

[Poly Haven 라이선스](https://polyhaven.com/license): CC0 1.0. 원본 재배포와 수정·상업적 사용을 허용합니다. [CC0 원문](https://creativecommons.org/publicdomain/zero/1.0/). 출처 표기는 추적과 인수인계를 위해 보존합니다. GitHub에 공개돼 있다는 이유만으로 임의의 다른 에셋을 사용하지 않습니다.

`Textures/PolyHaven/manifest.json`에는 파일별 다운로드 URL, SHA-256, 제작자, 라이선스가 기록돼 있습니다. 원본은 각각 2K diffuse, 1K OpenGL normal, 1K DirectX normal, 1K roughness입니다. Unity 폴더의 동일 JPG는 원본 복사본이고 `*_metallic_smoothness.png`는 에디터가 만드는 데이터 채널 변환본입니다.

## Unity 적용

UnityWeb README대로 Prepare Project를 실행하면 임포트 설정과 두 Standard 머티리얼이 생성됩니다. 컬러만 sRGB, 노멀은 NormalMap, 거칠기와 Smoothness는 linear입니다. OpenGL 노멀을 사용합니다. 미터 기준 면별 UV는 SurfaceArt.cs가 런타임에 생성합니다. 기존 동선·충돌을 유지하고 장식만 추가했습니다.

## Unreal 적용 절차 — 에디터 실행 미검증

1. UE 빌드와 `Content/Python/build_duskline.py` 맵 생성이 끝난 뒤 `/Game/Maps/Duskline`을 엽니다.
2. Content Browser에 `/Game/Art/PolyHaven` 폴더를 만들고 각 에셋의 `diff_2k`, `nor_dx_1k`, `rough_1k` JPG를 가져옵니다. GL 노멀 대신 **DX 노멀**을 사용합니다.
3. diffuse는 sRGB 사용, roughness는 sRGB 해제, normal은 Compression Settings=Normalmap으로 확인합니다.
4. `M_Asphalt`, `M_Concrete` 머티리얼을 만듭니다. 각각 diffuse RGB→Base Color, normal RGB→Normal, roughness R→Roughness, Metallic=0으로 연결합니다. Unity용 Smoothness PNG를 Unreal Roughness에 직접 연결하지 않습니다.
5. 큰 기본 큐브에 텍스처가 늘어나지 않도록 WorldAlignedTexture(컬러·거칠기)와 WorldAlignedNormal(노멀)을 사용해 월드 좌표 투영을 구성합니다. Texture Object를 입력하고 TextureSize는 아스팔트 `(400,400,400)`cm, 콘크리트 `(300,300,300)`cm부터 조정합니다. 사용하는 UE 버전의 함수 입력과 노멀 방향을 에디터에서 확인합니다.
6. Ground의 Static Mesh Component Material Element 0에 M_Asphalt를 배정합니다. Boundary, AlleyWest/East, NorthBlock, Room 벽/지붕, StartCover/EastCover에는 M_Concrete를 배정합니다. 물자·플레이어·탈출 비콘은 기존 식별 색을 유지합니다.
7. 머티리얼 셰이더 컴파일을 확인하고 맵을 저장합니다. PIE에서 표면 스케일·노멀 방향·조명, 충돌·NavMesh·탈출을 확인한 후 정확한 UE 버전과 결과를 루트 README에 기록합니다.

Unity 화면의 창틀·조명 변경은 Unity의 WorldBootstrap에 구현되어 있으므로 Unreal 맵에 자동으로 생기지 않습니다. 이 안내는 공통 재질을 적용하는 인수인계 절차이며 Unreal에서 검증된 렌더 결과를 의미하지 않습니다.
