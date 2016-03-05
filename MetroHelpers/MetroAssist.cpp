// Class1.cpp
#include "pch.h"
#include "MetroAssist.h"
#include <agile.h>
#include <windows.ui.xaml.media.dxinterop.h>

using namespace Microsoft::WRL;
using namespace Windows::Foundation;
using namespace MetroHelpers;
using namespace Platform;
using namespace D2D1;

MetroAssist::MetroAssist()
{
	// This flag adds support for surfaces with a different color channel ordering than the API default.
	// It is recommended usage, and is required for compatibility with Direct2D.
	UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;	

#if defined(_DEBUG)
	// If the project is in a debug build, enable debugging via SDK Layers with this flag.
	creationFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif

	// This array defines the set of DirectX hardware feature levels this app will support.
	// Note the ordering should be preserved.
	// Don't forget to declare your application's minimum required feature level in its
	// description.  All applications are assumed to support 9.1 unless otherwise stated.
	D3D_FEATURE_LEVEL featureLevels[] = 
	{
		D3D_FEATURE_LEVEL_11_1,
		D3D_FEATURE_LEVEL_11_0,
		D3D_FEATURE_LEVEL_10_1,
		D3D_FEATURE_LEVEL_10_0,
		D3D_FEATURE_LEVEL_9_3,
		D3D_FEATURE_LEVEL_9_2,
		D3D_FEATURE_LEVEL_9_1
	};

	// Create the DX11 API device object, and get a corresponding context.
	ComPtr<ID3D11Device> device;
	ComPtr<ID3D11DeviceContext> context;
	DX::ThrowIfFailed(
		D3D11CreateDevice(
			nullptr,                    // specify null to use the default adapter
			D3D_DRIVER_TYPE_HARDWARE,
			0,                          // leave as 0 unless software device
			creationFlags,              // optionally set debug and Direct2D compatibility flags
			featureLevels,              // list of feature levels this app can support
			ARRAYSIZE(featureLevels),   // number of entries in above list
			D3D11_SDK_VERSION,          // always set this to D3D11_SDK_VERSION for Metro style apps
			&device,                    // returns the Direct3D device created
			&m_featureLevel,            // returns feature level of device created
			&context                    // returns the device immediate context
			)
		);

	// Get the DirectX11.1 device by QI off the DirectX11 one.
	DX::ThrowIfFailed(
		device.As(&m_d3dDevice)
		);
	DX::ThrowIfFailed(
		m_d3dDevice.As(&dxgiDevice)
		);
}

Point MetroAssist::ScreenResolution::get()
{
	// Obtain the underlying DXGI device of the Direct3D11.1 device.
	// Identify the physical adapter (GPU or card) this device is running on.
	ComPtr<IDXGIAdapter> dxgiAdapter;
	DX::ThrowIfFailed(
		dxgiDevice->GetAdapter(&dxgiAdapter)
		);
	IDXGIOutput * pOutput;
	if (dxgiAdapter->EnumOutputs(0, &pOutput) != DXGI_ERROR_NOT_FOUND)
	{
		DXGI_OUTPUT_DESC desc;
		pOutput->GetDesc(&desc);
		return Point(desc.DesktopCoordinates.right, desc.DesktopCoordinates.bottom);
	}

	return Point(0, 0);
}