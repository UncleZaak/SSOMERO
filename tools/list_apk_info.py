import zipfile, sys

files = [
	r"C:/Users/ISAAC L KABUYE/source/repos/Ssomero/artifacts/release/com.companyname.ssomero.apk",
	r"C:/Users/ISAAC L KABUYE/source/repos/Ssomero/artifacts/release/com.companyname.ssomero-Signed.apk",
]

for f in files:
	print('\n== {} =='.format(f))
	try:
		with zipfile.ZipFile(f,'r') as z:
			names = z.namelist()
			# META-INF signature files
			sigs = [n for n in names if n.upper().startswith('META-INF/') and (n.upper().endswith('.SF') or n.upper().endswith('.RSA') or n.upper().endswith('.DSA') or n.upper().endswith('.MF'))]
			print('Signature-related META-INF files:', sigs if sigs else 'None')
			# APK Sig Block check (raw search)
			data = None
			try:
				with open(f,'rb') as fh:
					data = fh.read()
			except Exception:
				pass
			if data and b'APK Sig Block' in data:
				print('APK Sig Block present (v2/v3 signature likely)')
			else:
				print('No APK Sig Block detected')
			# list lib ABIs
			libs = sorted({n.split('/')[1] for n in names if n.startswith('lib/')})
			print('lib ABIs found:', libs if libs else 'None')
			# manifest presence
			manifest_names = [n for n in names if n.lower().endswith('androidmanifest.xml')]
			print('AndroidManifest entries:', manifest_names if manifest_names else 'None')
			# count native libs
			native_count = len([n for n in names if n.startswith('lib/') and n.endswith('.so')])
			print('Native libraries (.so) count:', native_count)
	except Exception as e:
		print('Error reading', f, e)

print('\nDone')
