window.addEventListener('load', async () => {
  const response = await fetch('/stream');
  const readableStream = response.body;
  const reader = readableStream.getReader();
  let result;
  let url;
  let timestamp = performance.now();
  const blobImg = document.querySelector('#blobImg');
  while ((result = await reader.read()) && !result.done) {
    const blob = new Blob([result.value]);
    URL.revokeObjectURL(url);
    url = URL.createObjectURL(blob);
    blobImg.src = url;
    document.title = `${Math.round(1000 / (performance.now() - timestamp))} FPS`;
    timestamp = performance.now();
  };
});
