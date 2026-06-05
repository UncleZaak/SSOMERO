import hashlib
import zipfile

files = [
	r"C:/Users/ISAAC L KABUYE/source/repos/Ssomero/artifacts/release/com.companyname.ssomero.apk",
	r"C:/Users/ISAAC L KABUYE/source/repos/Ssomero/artifacts/release/com.companyname.ssomero-Signed.apk",
]

def sha256(path):
	h=hashlib.sha256()
	with open(path,'rb') as f:
		for chunk in iter(lambda: f.read(8192), b''):
			h.update(chunk)
	return h.hexdigest()

for f in files:
	print('\n== {} =='.format(f))
	try:
		import os
		st = os.stat(f)
		print('Size:', st.st_size)
		print('SHA256:', sha256(f))
		# check for APK Sig Block magic
		with open(f,'rb') as fh:
			data = fh.read()
			if b'APK Sig Block' in data:
				print('Contains APK Sig Block (v2/v3 signature likely present)')
			else:
				print('No APK Sig Block found')
		# inspect META-INF
		with zipfile.ZipFile(f,'r') as z:
			meta = [e for e in z.namelist() if e.upper().startswith('META-INF/')]
			print('META-INF count:', len(meta))
			sig_files = [m for m in meta if m.upper().endswith('.SF') or m.upper().endswith('.RSA') or m.upper().endswith('.DSA') or m.upper().endswith('.MF')]
			if sig_files:
				print('Signature-related META-INF files:')
				for s in sig_files:
					print('  ', s)
			else:
				print('No signature-related META-INF files detected')
			# show if AndroidManifest.xml present
			am = [name for name in z.namelist() if name.lower().endswith('androidmanifest.xml')]
			print('AndroidManifest entries:', am[:5])
	except Exception as e:
		print('Error inspecting', f, e)

print('\nComparison')
try:
	import os
	f1=files[0]; f2=files[1]
	s1=os.stat(f1).st_size; s2=os.stat(f2).st_size
	print(f1, 'size', s1)
	print(f2, 'size', s2)
	if s1==s2:
		print('Sizes equal')
	else:
		print('Sizes differ')
except Exception:
	pass
