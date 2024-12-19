function previewImage() {
    var oFReader = new FileReader();
    oFReader.readAsDataURL(document.getElementById("upload").files[0]);

    oFReader.onload = function (oFREvent) {
        document.getElementById("uploadedAvatar").src = oFREvent.target.result;
    };
}
