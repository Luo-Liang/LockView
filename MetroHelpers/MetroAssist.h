#pragma once
#include <DXGI1_3.h>
namespace DX
{
	inline void ThrowIfFailed(HRESULT hr)
	{
		if (FAILED(hr))
		{
			// Set a breakpoint on this line to catch DirectX API errors
			throw Platform::Exception::CreateException(hr);
		}
	}
}

namespace MetroHelpers
{
    public ref class MetroAssist sealed
    {
    public:
        MetroAssist();
		property Windows::Foundation::Point ScreenResolution
		{
			Windows::Foundation::Point get();
		}

		void DelegatedCallTrim()
		{
			dxgiDevice->Trim();
		}


	private:
		D3D_FEATURE_LEVEL							    m_featureLevel;
		Microsoft::WRL::ComPtr<ID3D11Device1>           m_d3dDevice;
		Microsoft::WRL::ComPtr<IDXGIDevice3> dxgiDevice;

    };
}