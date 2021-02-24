var FileReaderPlugin = {
  FileReaderCaptureClick: function() {
    if (!document.getElementById('FileReaderInput')) {
      var fileInput = document.createElement('input');
      fileInput.setAttribute('type', 'file');
      fileInput.setAttribute('id', 'FileReaderInput');
      fileInput.setAttribute('multiple', 'multiple')
      fileInput.style.visibility = 'hidden';
      fileInput.onclick = function (event) {
        this.value = null;
      };
      fileInput.onchange = function (e) {
        if (!e.target.files[0]) {
            return;
        }
        var files = e.target.files;
        
        var sendContent = function(e) {
            var returnStr = e.target.result;
            
            SendMessage('Button', 'FileSelected', returnStr);
        };

        for (var i = 0; i < files.length; i++) {
          var reader = new FileReader();
          reader.onload = sendContent;
          reader.readAsText(files[i]);
        }
      }
      document.body.appendChild(fileInput);
    }
    var OpenFileDialog = function() {
      document.getElementById('FileReaderInput').click();
      document.getElementById('unity-canvas').removeEventListener('click', OpenFileDialog);
    };
    document.getElementById('unity-canvas').addEventListener('click', OpenFileDialog, false);
  }
};
mergeInto(LibraryManager.library, FileReaderPlugin);