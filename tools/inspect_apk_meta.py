import zipfile
import sys

files = [
	r"C:/Users/ISAAC L KABUYE/source/repos/Ssomero/artifacts/release/com.companyname.ssomero.apk",
	r"C:/Users/ISAAC L KABUYE/source/repos/Ssomero/artifacts/release/com.companyname.ssomero-Signed.apk",
]

for f in files:
	print('\n== {} =='.format(f))
	try:
		with zipfile.ZipFile(f,'r') as z:
			meta = [e for e in z.namelist() if e.upper().startswith('META-INF/')]
			if meta:
				for m in meta:
					print(m)
			else:
				print('No META-INF entries found')
			# check for AndroidManifest package string
			manifest = None
			try:
				manifest = z.read('AndroidManifest.xml')
			except KeyError:
				# try different capitalization
				for name in z.namelist():
					if name.endswith('AndroidManifest.xml'):
						manifest = z.read(name)
						break
			if manifest:
				try:
					s = manifest.decode('utf-8', errors='ignore')
					if 'package="' in s:
						# crude extract
						idx = s.find('package=')
						print('Manifest snippet around package:', s[idx:idx+100])
					else:
						# search for com.companyname.ssomero
						if 'com.companyname.ssomero' in s:
							print('Found package string in manifest text')
						else:
							print('No readable package string in manifest text')
				except Exception as e:
					print('Error decoding manifest:', e)
			else:
				print('AndroidManifest.xml not found in APK')
	except Exception as e:
		print('Error reading {}: {}'.format(f,e))

print('\nDone')
