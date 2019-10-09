Shader "Unlit/Texture"{
    Properties{
        // name(displayNameInInspector, TVAL_ID) = defaultValue        name的名字可以自定义，TVAL_ID必须是已有的
        // 属性可以在动态设置（Inspector面板中），也可以在C#代码中通过Material的相关方法进行设置
        _Color("Totally Rad Color", Color) = (1, 1, 1, 1)
        _MainTexture("MainTexture", 2D) = "white"{}
        _DissolveTexture("DissolveTexture", 2D) = "white"{}
        _DissolveCutoff("DissolveCutoff", Range(0, 1)) = 1              // float值，范围0~1
        _ExtrudeAmount("ExtrudeAmount", float) = 0
    }
    Subshader{
        Pass{
            CGPROGRAM
            #pragma vertex vertexFunction
            #pragma fragment fragmentFunction
            #include "UnityCG.cginc"

            // 获取属性以供后续使用
            float4 _Color;
            sampler2D _MainTexture;
            sampler2D _DissolveTexture;
            float _DissolveCutoff;
            float _ExtrudeAmount;

            // 结构体的名字是自定义的
            struct a2v2{
                // POSITION是固定的，表示位置，其他的同理
                // vertex这样的变量名可以自定义
                float4 vertex:POSITION;
                float2 uv:TEXCOORD0;
                float3 normal10:NORMAL;          // NORMAL是法线
            };
            struct v2f{
              float4 position:SV_POSITION;
              float2 uv:TEXCOORD0;
            };

            // 顶点函数，必须叫这个名字
            v2f vertexFunction(a2v2 v){
                v2f o;
                // _Time是一个代表时间的变量被包含在UnityCH.cginc中，y值代表秒，确保“Animated Materials” 在场景视图中被勾选
                v.vertex.xyz += v.normal10.xyz * _ExtrudeAmount * sin(_Time.y);
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            // 片元函数，返回RGB名字这个名字
            fixed4 fragmentFunction(v2f i):SV_TARGET{
                // return fixed4(0, 1, 0, 1);
                // tex2D函数可以对纹理进行采样
                float4 textureColor = tex2D(_MainTexture, i.uv);
                // float4 dissolveColor = tex2D(_DissolveTexture, i.uv);
                // clip 函数检查这个给定的值是否小于0.如果小于0，我们将丢弃这个像素并且不绘制它。如果大于0，继续保持像素、正常渲染
                // clip(dissolveColor.rgb - _DissolveCutoff);
                // return _Color;
                return textureColor;
            }
            ENDCG
        }
    }
}